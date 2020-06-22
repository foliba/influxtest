using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using InfluxDB.Client.Core;
using InfluxDB.LineProtocol.Payload;

namespace DTOs
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    // properties of DTOs are always public
    public abstract class PlanBaseDTO
    {
        //    tags
        [Column("planID", IsTag = true)] public string PlanId { get; set; }
        [Column("userId", IsTag = true)] public string UserId { get; set; }
        [Column("planVersion", IsTag = true)] public virtual string PlanVersion { get; set; }
        
        //    fields
        [Column("comment")] public string Comment { get; set; }
        //[Column(name:"guid", IsTag = true)] public string GUID { get; set; }
        
        //    timestamp
        [Column(IsTimestamp = true)] public DateTime Time;
        
        protected PlanBaseDTO()
        {
            Time = DateTime.UtcNow;
            //GUID = Guid.NewGuid().ToString();
        }
        
        public LineProtocolPoint ToLineProtocolPoint()
        {
            var fields = new Dictionary<string, object>();
            var tags = new Dictionary<string, string>();

            var dtoType = GetType();
            var props = dtoType.GetProperties()
                .Select(property => new PropertyInfoColumn
                {
                    Column = (Column) property.GetCustomAttribute(typeof(Column)),
                    Property = property
                })
                .Where(propertyInfo => propertyInfo.Column != null)
                .ToArray();

            foreach (var propertyInfoColumn in props)
            {
                var value = propertyInfoColumn.Property.GetValue(this);
                if (value == null) continue;
                var name = !string.IsNullOrEmpty(propertyInfoColumn.Column.Name)
                    ? propertyInfoColumn.Column.Name
                    : propertyInfoColumn.Property.Name;

                if (propertyInfoColumn.Column.IsTag)
                    tags.Add(name, value.ToString());
                else if (propertyInfoColumn.Column.IsTimestamp) continue;

                fields.Add(name, value);
            }

            var attr = (Measurement) Attribute.GetCustomAttributes(dtoType)[0];

            return new LineProtocolPoint(
                attr.Name,
                fields,
                tags,
                Time);
        }

        private class PropertyInfoColumn
        {
            internal Column Column;
            internal PropertyInfo Property;
        }
    }
}