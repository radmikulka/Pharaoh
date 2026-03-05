using System;
using System.ComponentModel;
using System.Globalization;

namespace ServerData
{
    [TypeConverter(typeof(SDiscountStickerTypeConverter))]
    public class SDiscountSticker
    {
        public EDiscountStickerId Id;
        public int Value;

        public SDiscountSticker(EDiscountStickerId id, int value)
        {
            Id = id;
            Value = value;
        }

        public SDiscountSticker(string serialized)
        {
            string[] split = serialized.Split(';');
            Id = (EDiscountStickerId)int.Parse(split[0]);
            Value = int.Parse(split[1]);
        }
        
        public override string ToString()
        {
            return $"{(int)Id};{Value}";
        }
    }
    
    public class SDiscountStickerTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value is string casted
                ? new SDiscountSticker(casted)
                : base.ConvertFrom(context, culture, null);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return destinationType == typeof (string) && value is SDiscountStickerTypeConverter casted
                ? casted.ToString()
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }
}