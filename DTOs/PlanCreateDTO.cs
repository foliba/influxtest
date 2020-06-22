using InfluxDB.Client.Core;

namespace DTOs
{
    [Measurement("planCreated")]
    public class PlanCreateDTO : PlanBaseDTO
    {
        /// <summary>
        ///     We overwrite the base PlanVersion to make it a fields here.
        ///     This is done because events without a field are silently ignored by the influxDB client.
        /// </summary>
        [Column("planVersion")]
        public override string PlanVersion { get; set; }
    }
}