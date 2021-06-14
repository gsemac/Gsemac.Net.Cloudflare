using Newtonsoft.Json;
using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr.Json {

    internal class FlareSolverrCookieCollectionJsonConverter :
        JsonConverter {

        // Public members

        public override bool CanConvert(Type objectType) {

            return objectType.Equals(typeof(CookieCollection));

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            CookieCollection cookies = new CookieCollection();
            FlareSolverrCookieJsonConverter cookieJsonConverter = new FlareSolverrCookieJsonConverter();

            while (reader.TokenType != JsonToken.EndArray && reader.Read()) {

                if (reader.TokenType == JsonToken.StartObject)
                    cookies.Add(cookieJsonConverter.ReadJson(reader, objectType, existingValue, serializer) as Cookie);

            }

            return cookies;

        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            FlareSolverrCookieJsonConverter cookieJsonConverter = new FlareSolverrCookieJsonConverter();

            writer.WriteStartArray();

            foreach (Cookie cookie in value as CookieCollection)
                cookieJsonConverter.WriteJson(writer, cookie, serializer);

            writer.WriteEndArray();

        }

    }

}