using Newtonsoft.Json;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr.Json {

    internal sealed class FlareSolverrResponseStatusJsonConverter :
        JsonConverter {

        // Public members

        public override bool CanConvert(Type objectType) {

            return objectType.Equals(typeof(FlareSolverrResponseStatus));

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            string statusString = (string)reader.Value;

            switch (statusString.ToLowerInvariant()) {

                case "error":
                    return FlareSolverrResponseStatus.Error;

                default:
                    return FlareSolverrResponseStatus.Ok;

            }

        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            if (value is FlareSolverrResponseStatus responseStatus) {

                switch (responseStatus) {

                    case FlareSolverrResponseStatus.Ok:
                        writer.WriteValue("ok");
                        break;

                    case FlareSolverrResponseStatus.Error:
                        writer.WriteValue("error");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));

                }

            }
            else {

                throw new ArgumentException(nameof(value));

            }

        }

    }

}