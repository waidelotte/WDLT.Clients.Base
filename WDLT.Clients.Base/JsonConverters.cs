﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace WDLT.Clients.Base
{
    public class MicrosecondEpochConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            return DateTimeOffset.FromUnixTimeMilliseconds((long)reader.Value);
        }
    }

    public class ArrayedObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Object)
            {
                return token.ToObject(objectType);
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class LongToDoublePriceJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.ReadFrom(reader);
            var l = jt.Value<long>();

            return Math.Round((double)l / 100, 2);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(double) == objectType;
        }
    }

    public class PriceToLongJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.ReadFrom(reader);
            var l = jt.Value<double>();

            return (long)(l * 100);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(long) == objectType;
        }
    }
}