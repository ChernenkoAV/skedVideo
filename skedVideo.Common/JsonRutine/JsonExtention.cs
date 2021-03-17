using System;
using System.Collections;
using System.Collections.Generic;
using Cav;
using JsonSettings;
using Newtonsoft.Json;

namespace skedVideo
{
    public static class JsonExtention
    {
        /// <summary>
        /// Json сериализация. null не выводятся. Пустые <see cref="IList"/> тождественны null
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static String JsonSerealize(this Object obj)
        {
            if (obj == null)
                return null;

            if ((obj as IList)?.Count == 0)
                return null;

            return JsonConvert.SerializeObject(obj, GenericJsonSerializerSetting.Instance);
        }

        /// <summary>
        /// Json десериализация. возврат: Если тип реализует <see cref="IList"/> - пустую коллекцию(что б в коде не проверять на null и сразу юзать foreach)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        public static T JsonDeserealize<T>(this String s)
        {
            return (T)s.JsonDeserealize(typeof(T));
        }

        /// <summary>
        /// Json десериализация. возврат: Если тип реализует <see cref="IList"/> - пустую коллекцию(что б в коде не проверять на null и сразу юзать foreach)
        /// </summary>
        public static object JsonDeserealize(this String s, Type type)
        {
            if (s.IsNullOrWhiteSpace())
            {
                if (type.IsArray)
                    return Array.CreateInstance(type.GetElementType(), 0);

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return Array.CreateInstance(type.GetGenericArguments()[0], 0);

                if (typeof(IList).IsAssignableFrom(type))
                    return Activator.CreateInstance(type);

                return type.GetDefault();
            }

            return JsonConvert.DeserializeObject(s, type, GenericJsonSerializerSetting.Instance);
        }
    }
}
