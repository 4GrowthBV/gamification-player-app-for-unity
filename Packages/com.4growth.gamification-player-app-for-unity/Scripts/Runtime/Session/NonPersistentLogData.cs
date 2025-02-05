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

        // This is the new dictionary that will hold all of the listeners.
        private Dictionary<Type, List<Action<object>>> listeners = new();

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

        private bool HasLatestQueryableValue(Type attributeType, object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Type itemType = obj.GetType();
            System.Reflection.FieldInfo[] fields = itemType.GetFields();
            System.Reflection.FieldInfo fieldWithAttribute = fields.FirstOrDefault(f => Attribute.IsDefined(f, attributeType));
            if (fieldWithAttribute != null)
            {
                return true;
            }
            else
            {
                foreach (var child in fields.Where(f => f.FieldType.Name != "String" && !f.FieldType.IsPrimitive))
                {
                    if (HasLatestQueryableValue(attributeType, child.GetValue(obj)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private object GetLatestQueryableValue(Type attributeType, object obj)
        {
            if (obj == null)
            {
                return null;
            }

            Type itemType = obj.GetType();
            System.Reflection.FieldInfo[] fields = itemType.GetFields();
            System.Reflection.FieldInfo fieldWithAttribute = fields.FirstOrDefault(f => Attribute.IsDefined(f, attributeType));
            if (fieldWithAttribute != null)
            {
                return fieldWithAttribute.GetValue(obj);
            }
            else
            {
                foreach (var child in fields.Where(f => f.FieldType.Name != "String" && !f.FieldType.IsPrimitive))
                {
                    if(HasLatestQueryableValue(attributeType, child.GetValue(obj)))
                    {
                        return GetLatestQueryableValue(attributeType, child.GetValue(obj));
                    }
                }
            }

            return null;
        }


        public void AddToLog(ILoggableData dto)
        {
            dto.Time = Time.realtimeSinceStartup;
            
            logData.Add(dto);

            // Check if the newly added dto has any listeners.
            foreach (var pair in listeners)
            {
                if(HasLatestQueryableValue(pair.Key, dto))
                {   
                    foreach(var listener in pair.Value)
                    {
                        listener(GetLatestQueryableValue(pair.Key, dto));
                    }
                }
            }
        }

        public void AddToLog(IEnumerable<ILoggableData> dtos)
        {
            foreach(var dto in dtos)
            {
                dto.Time = Time.realtimeSinceStartup;
            }

            logData.AddRange(dtos);

            foreach(var dto in dtos)
            {
                // Check if the newly added dto has any listeners.
                foreach (var pair in listeners)
                {
                    if(HasLatestQueryableValue(pair.Key, dto))
                    {   
                        foreach(var listener in pair.Value)
                        {
                            listener(GetLatestQueryableValue(pair.Key, dto));
                        }
                    }
                }
            }
        }

        // Method to register listeners.
        public void ListenTo<T>(Action<object> callback) where T : Session.IQueryable
        {
            // If a listener for this IQueryable type doesn't already exist, add it.
            if (!listeners.ContainsKey(typeof(T)))
            {
                listeners.Add(typeof(T), new List<Action<object>>() { callback });
            }
            else
            {
                // Otherwise, just update the existing listener.
                listeners[typeof(T)].Add(callback);
            }
        }

        public void RemoveListener(Action<object> callback)
        {
            foreach (var pair in listeners)
            {
                if(pair.Value.Contains(callback))
                {
                    pair.Value.Remove(callback);
                }
            }
        }

        public void ClearData()
        {
            logData.Clear();
        }
    }
}
