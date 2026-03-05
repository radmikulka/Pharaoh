// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.ComponentModel;

namespace ServerData
{
    public class COfferParam : IOfferParam
    {
        public EOfferParam Id { get; }
        public string StringValue { get; }
        
        public static COfferParam New<T>(EOfferParam id, T value)
        {
            if(value is string str)
                return new COfferParam(id, str);

            if (value is Enum)
                return new COfferParam(id, Convert.ToInt32(value).ToString());
            
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            string stringValue = converter.ConvertToString(value);
            return new COfferParam(id, stringValue);
        }
        
        private COfferParam(EOfferParam id, string stringValue)
        {
            StringValue = stringValue;
            Id = id;
        }
        
        public T GetValue<T>()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            return (T) converter.ConvertFromString(StringValue);
        }
        
        public T GetValueOrDefault<T>()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            object result = converter.ConvertFromString(StringValue);
            if(result is null)
                return default;
            return (T) result;
        }
    }
}