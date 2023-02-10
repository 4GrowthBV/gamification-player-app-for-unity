using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamificationPlayer
{
    public class NonPersistentLogData
    { 
        public IEnumerable<ILoggableData> LogData
        {
            get
            {
                return logData.AsReadOnly();
            }
        }

        private List<ILoggableData> logData = new List<ILoggableData>();

        public bool TryGetLatestQueryableValue<TValue, TAttribute>(out TValue value)
            where TAttribute : Session.IQueryable
        {
            var data = logData.Where(i => HasLatestQueryableValue<TValue, TAttribute>(i))
                .Select(i => GetLatestQueryableValue<TValue, TAttribute>(i));

            if(data.Count() == 0)
            {
                value = default;

                return false;
            }

            object fieldValue = data.Last();

            value = (TValue)fieldValue;

            return true;
        }

        private bool HasLatestQueryableValue<TValue, TAttribute>(object obj)
            where TAttribute : Session.IQueryable
        {
            if (obj == null)
            {
                return false;
            }

            Type itemType = obj.GetType();
            System.Reflection.FieldInfo[] fields = itemType.GetFields();
            System.Reflection.FieldInfo fieldWithAttribute = fields.FirstOrDefault(f => Attribute.IsDefined(f, typeof(TAttribute)));
            if (fieldWithAttribute != null)
            {
                return true;
            }
            else
            {
                foreach (var child in fields.Where(f => f.FieldType.Name != "String" && !f.FieldType.IsPrimitive))
                {
                    if (HasLatestQueryableValue<TValue, TAttribute>(child.GetValue(obj)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private TValue GetLatestQueryableValue<TValue, TAttribute>(object obj)
            where TAttribute : Session.IQueryable
        {
            if (obj == null)
            {
                return default;
            }

            Type itemType = obj.GetType();
            System.Reflection.FieldInfo[] fields = itemType.GetFields();
            System.Reflection.FieldInfo fieldWithAttribute = fields.FirstOrDefault(f => Attribute.IsDefined(f, typeof(TAttribute)));
            if (fieldWithAttribute != null)
            {
                return (TValue)fieldWithAttribute.GetValue(obj);
            }
            else
            {
                foreach (var child in fields.Where(f => f.FieldType.Name != "String" && !f.FieldType.IsPrimitive))
                {
                    if(HasLatestQueryableValue<TValue, TAttribute>(child.GetValue(obj)))
                    {
                        return GetLatestQueryableValue<TValue, TAttribute>(child.GetValue(obj));
                    }
                }
            }

            return default;
        }

        public void AddToLog(ILoggableData dto)
        {
            dto.Time = Time.realtimeSinceStartup;
            
            logData.Add(dto);
        }

        public void AddToLog(IEnumerable<ILoggableData> dtos)
        {
            foreach(var dto in dtos)
            {
                dto.Time = Time.realtimeSinceStartup;
            }

            logData.AddRange(dtos);
        }
    }
}
