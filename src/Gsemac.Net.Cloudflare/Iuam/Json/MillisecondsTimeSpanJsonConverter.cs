using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gsemac.Net.Cloudflare.Iuam.Json {

    public class MillisecondsTimeSpanJsonConverter :
        JsonConverter {

        public override bool CanConvert(Type objectType) {

            return objectType.Equals(typeof(TimeSpan));

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            return TimeSpan.FromMilliseconds(reader.ReadAsDouble().Value);

        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            if (value is TimeSpan timeSpan) {

                writer.WriteValue((long)timeSpan.TotalMilliseconds);

            }
            else
                throw new ArgumentException(nameof(value));

        }

    }

}