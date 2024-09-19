using Newtonsoft.Json;
using System;

namespace Gsemac.Cloudflare.CFClearanceScraper.Json {

    internal sealed class CfClearanceScraperModeJsonConverter :
        JsonConverter {

        // Public members

        public override bool CanConvert(Type objectType) {

            if (objectType is null)
                return false;

            return objectType.Equals(typeof(CfClearanceScraperMode));

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            if (reader is null)
                throw new ArgumentNullException(nameof(reader));

            if (objectType is null)
                throw new ArgumentNullException(nameof(objectType));

            if (serializer is null)
                throw new ArgumentNullException(nameof(serializer));

            string value = (string)reader.Value;

            switch (value.ToLowerInvariant().Trim()) {

                case "captcha":
                    return CfClearanceScraperMode.Captcha;

                default:
                    return CfClearanceScraperMode.Waf;

            }

        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (serializer is null)
                throw new ArgumentNullException(nameof(serializer));

            if (value is CfClearanceScraperMode mode) {

                switch (value) {

                    case CfClearanceScraperMode.Waf:
                        writer.WriteValue("waf");
                        break;

                    case CfClearanceScraperMode.Captcha:
                        writer.WriteValue("captcha");
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