using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace IngestOldClient
{
    internal static class Program
    {
        private const int PlanAmount = 1;
        private const int ChangeEventPerPlanAmount = 1;
        private static readonly string DBName = "db0";
        private static readonly string DBUrl = "http://localhost:8086";
        private static readonly List<string> PlanIDs = new List<string>();

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Old World!");

            CollectorLog.RegisterErrorHandler((message, exception) =>
            {
                Console.WriteLine($"{message}: {exception}");
            });

            var client = new LineProtocolClient(new Uri(DBUrl), DBName);

            var createEvents = new LineProtocolPayload();

            for (var i = 0; i < PlanAmount; i++)
                createEvents.Add(Create_PlanCreateDTO().ToLineProtocolPoint());

            var influxResult = await client.WriteAsync(createEvents);
            if (!influxResult.Success)
                await Console.Error.WriteLineAsync(influxResult.ErrorMessage);

            var changeEvents = new LineProtocolPayload();
            PlanIDs.ForEach(planId =>
            {
                for (var i = 0; i < ChangeEventPerPlanAmount; i++)
                    changeEvents.Add(Create_PlanChangeValueDTO(planId, i).ToLineProtocolPoint());
            });
            influxResult = await client.WriteAsync(changeEvents);
            if (!influxResult.Success)
                await Console.Error.WriteLineAsync(influxResult.ErrorMessage);

            var deleteEvents = new LineProtocolPayload();
            PlanIDs.ForEach(planId =>
            {
                deleteEvents.Add(new PlanDeleteDTO
                {
                    PlanId = planId,
                    UserId = "1234567",
                    PlanVersion = new Version(1, 2).ToString()
                }.ToLineProtocolPoint());
            });
            influxResult = await client.WriteAsync(deleteEvents);
            if (!influxResult.Success)
                await Console.Error.WriteLineAsync(influxResult.ErrorMessage);
        }

        private static PlanCreateDTO Create_PlanCreateDTO()
        {
            var planCreated = new PlanCreateDTO
            {
                PlanId = Guid.NewGuid().ToString(),
                UserId = "12345",
                PlanVersion = new Version(1, 2).ToString()
            };

            if (!PlanIDs.Contains(planCreated.PlanId)) PlanIDs.Add(planCreated.PlanId);

            return planCreated;
        }

        private static PlanChangeValueDTO Create_PlanChangeValueDTO(string planId, int i)
        {
            var propertyName = (i % 5) switch
            {
                0 => PlanProperty.Prop0.ToString(),
                1 => PlanProperty.Prop1.ToString(),
                2 => PlanProperty.Prop2.ToString(),
                3 => PlanProperty.Prop3.ToString(),
                4 => PlanProperty.Prop4.ToString(),
                _ => string.Empty
            };

            return new PlanChangeValueDTO
            {
                PlanId = planId,
                UserId = "123456",
                PlanVersion = new Version(1, 2).ToString(),
                PropertyName = propertyName,
                OldValue = "oldValue1",
                NewValue = "newValue2"
            };
        }

        private enum PlanProperty
        {
            Prop0,
            Prop1,
            Prop2,
            Prop3,
            Prop4
        }
    }
}