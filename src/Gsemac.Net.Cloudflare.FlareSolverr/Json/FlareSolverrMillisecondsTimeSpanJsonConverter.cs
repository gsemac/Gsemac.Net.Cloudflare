using Newtonsoft.Json;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr.Json {

    internal sealed class FlareSolverrMillisecondsTimeSpanJsonConverter :
        JsonConverter {

        // Public members

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