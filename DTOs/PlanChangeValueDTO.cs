using InfluxDB.Client.Core;

namespace DTOs
{
    [Measurement("planValueChanged")]
    public class PlanChangeValueDTO : PlanBaseDTO
    {
        //    tags
        [Column("propertyName", IsTag = true)] public string PropertyName { get; set; }
        
        //    fields
        [Column("oldValue")] public string OldValue { get; set; }
        [Column("newValue")] public string NewValue { get; set; }
    }
}