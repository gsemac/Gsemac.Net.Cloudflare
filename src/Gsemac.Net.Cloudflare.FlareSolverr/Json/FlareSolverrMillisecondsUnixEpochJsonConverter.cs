using Gsemac.Core;
using Newtonsoft.Json;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr.Json {

    internal class FlareSolverrMillisecondsUnixEpochJsonConverter :
        JsonConverter {

        public override bool CanConvert(Type objectType) {

            return objectType.Equals(typeof(DateTime)) || objectType.Equals(typeof(DateTimeOffset));

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            long unixTimeMilliseconds = (long)reader.Value;
            DateTimeOffset dateTimeOffset = DateUtilities.FromUnixTimeMilliseconds(unixTimeMilliseconds);

            if (objectType.Equals(typeof(DateTime))) {

                return dateTimeOffset.DateTime;

            }
            else if (objectType.Equals(typeof(DateTimeOffset))) {

                return dateTimeOffset;

            }
            else
                throw new ArgumentException(nameof(objectType));

        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            if (value is DateTime dateTime) {

                writer.WriteValue(DateUtilities.ToUnixTimeMilliseconds(dateTime));

            }
            else if (value is DateTimeOffset dateTimeOffset) {

                writer.WriteValue(DateUtilities.ToUnixTimeMilliseconds(dateTimeOffset));

            }
            else
                throw new ArgumentException(nameof(value));

        }

    }

}