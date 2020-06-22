using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Collector.Diagnostics;

namespace IngestNewClient
{
    internal class Program
    {
        private static readonly char[] Token = "".ToCharArray();
        private static readonly string BucketName = "db0/autogen";
        private static readonly string DbUrl = "http://localhost:8086";
        private static readonly List<string> PlanIDs = new List<string>();

        private static readonly string OrgId = "org_id";

        private static async Task Main(string[] args)
        {
            CollectorLog.RegisterErrorHandler((message, exception) =>
            {
                Console.WriteLine($"{message}: {exception}");
            });

            Console.WriteLine("Hello New World!");

            var influxDBClient = InfluxDBClientFactory.Create(DbUrl, Token);

            using (var writeApi = influxDBClient.GetWriteApi())
            {
                //    create events
                for (var i = 0; i < 1; i++)
                    writeApi.WriteMeasurement(BucketName, OrgId, WritePrecision.Ns, Create_PlanCreateDTO());

                //    change events
                PlanIDs.ForEach(planId =>
                {
                    for (var i = 0; i < 5; i++)
                        writeApi.WriteMeasurement(BucketName, OrgId, WritePrecision.Ns,
                            Create_PlanChangeValueDTO(planId, i));
                });

                //    delete events
                PlanIDs.ForEach(planId => writeApi.WriteMeasurement(BucketName, OrgId, WritePrecision.Ns,
                    new PlanDeleteDTO
                    {
                        PlanId = planId,
                        UserId = "12345",
                        PlanVersion = new Version(1, 2).ToString()
                    })
                );

                writeApi.Dispose();
            }

            await Read(influxDBClient);
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
                UserId = "123345678",
                PropertyName = propertyName,
                OldValue = "oldValue1",
                NewValue = "newValue2"
            };
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

        private static async Task Read(InfluxDBClient influxDBClient)
        {
            var flux = $"from(bucket:\"{BucketName}\") |> range(start: 0)";

            var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(flux, OrgId);
            fluxTables.ForEach(fluxTable =>
            {
                var fluxRecords = fluxTable.Records;

                fluxRecords.ForEach(fluxRecord =>
                {
                    var msg = $"{fluxRecord.GetTime()}: {fluxRecord.GetMeasurement()}";
                    foreach (var (key, value) in fluxRecord.Values)
                        if (!key.StartsWith('_') && key != "result" && key != "table")
                            msg += $" {key}:{value}";
                    msg += $" field: {fluxRecord.GetField()}";

                    Console.WriteLine(msg);
                });
            });
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