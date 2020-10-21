using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Cav;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace JsonSettings
{
    internal class FlagEnumStringConverter : StringEnumConverter
    {
        public FlagEnumStringConverter()
        {
            this.AllowIntegerValues = true;
        }

        public override bool CanConvert(Type objectType)
        {
            return base.CanConvert(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumType = Nullable.GetUnderlyingType(objectType) ?? objectType;

            var isFlag = enumType.GetCustomAttribute<FlagsAttribute>() != null;

            if (isFlag)
            {

                if (reader.TokenType == JsonToken.Integer)
                    return Enum.ToObject(enumType, serializer.Deserialize<int>(reader));

                return Enum.ToObject(
                    enumType,
                    serializer.Deserialize<string[]>(reader)
                        .Select(x => Enum.Parse(enumType, x))
                        .Aggregate(0, (cur, val) => cur | (int)val));
            }
            else
                return base.ReadJson(reader, enumType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var isFlag = value.GetType().GetCustomAttributes(typeof(FlagsAttribute), false).Any();

            if (isFlag)
                serializer.Serialize(writer, (value as Enum).FlagToList().Select(x => x.ToString()).ToArray());
            else
                base.WriteJson(writer, value, serializer);
        }
    }

    internal class NullListValueProvider : IValueProvider
    {
        private JsonProperty jsonProperty;
        private IValueProvider valueProvider;

        public NullListValueProvider(JsonProperty jsonProperty)
        {
            this.jsonProperty = jsonProperty;
            this.valueProvider = jsonProperty.ValueProvider;
        }
        public object GetValue(object target)
        {
            return valueProvider.GetValue(target);
        }

        public void SetValue(object target, object value)
        {
            if (value == null)
            {
                if (jsonProperty.PropertyType.IsArray)
                    value = Array.CreateInstance(jsonProperty.PropertyType.GetElementType(), 0);
                else
                    value = Activator.CreateInstance(jsonProperty.PropertyType);
            }

            valueProvider.SetValue(target, value);
        }
    }

    internal class CustomJsonContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Обрабатывать свойства, имеющие атрибут <see cref="ElasticMappingIgnoreAttribute"/>
        /// </summary>
        public Boolean ShouldElasticIgnore { get; set; }
        public override JsonContract ResolveContract(Type type)
        {
            return base.ResolveContract(type);
        }
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);

            if (typeof(IList).IsAssignableFrom(jProperty.PropertyType))
            {
                var olsShouldSerialise = jProperty.ShouldSerialize;
                if (olsShouldSerialise == null)
                    olsShouldSerialise = x => true;

                jProperty.ShouldSerialize = obj => olsShouldSerialise(obj) && ((jProperty.ValueProvider.GetValue(obj) as IList)?.Count ?? 0) != 0;

                jProperty.ValueProvider = new NullListValueProvider(jProperty);
                jProperty.NullValueHandling = NullValueHandling.Include;
                jProperty.DefaultValueHandling = DefaultValueHandling.Populate;
            }

            return jProperty;
        }
    }

    public class ElasticJsonSerializerSetting : JsonSerializerSettings
    {
        private static ElasticJsonSerializerSetting instance;

        public static ElasticJsonSerializerSetting Instance
        {
            get
            {
                if (instance == null)
                    instance = new ElasticJsonSerializerSetting();

                return instance;
            }
        }

        private ElasticJsonSerializerSetting()
        {
            this.NullValueHandling = NullValueHandling.Ignore;
            this.DefaultValueHandling = DefaultValueHandling.Include;
            this.Converters.Add(new FlagEnumStringConverter());
            this.ContractResolver = new CustomJsonContractResolver() { ShouldElasticIgnore = true };
        }
    }

    /// <summary>
    /// Общая настройка сериализатора Json
    /// </summary>
    public class GenericJsonSerializerSetting : JsonSerializerSettings
    {
        private static GenericJsonSerializerSetting instance;

        public static GenericJsonSerializerSetting Instance
        {
            get
            {
                if (instance == null)
                    instance = new GenericJsonSerializerSetting();

                return instance;
            }
        }

        private GenericJsonSerializerSetting()
        {
            this.NullValueHandling = NullValueHandling.Ignore;
            this.DefaultValueHandling = DefaultValueHandling.Ignore;
            this.Converters.Add(new FlagEnumStringConverter());
            this.ContractResolver = new CustomJsonContractResolver();
        }
    }
}
