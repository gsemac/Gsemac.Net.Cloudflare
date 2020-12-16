using Gsemac.Core;
using Gsemac.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam.Json {

    internal class FlareSolverrDataCookieJsonConverter :
        JsonConverter {

        // Public members

        public override bool CanConvert(Type objectType) {

            return objectType.Equals(typeof(Cookie));

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            Cookie cookie = new Cookie();

            while (reader.TokenType != JsonToken.EndObject && reader.Read()) {

                if (reader.TokenType == JsonToken.PropertyName) {

                    // Read the property name and value.

                    string property = reader.ReadAsString();

                    reader.Read();

                    string value = reader.ReadAsString();

                    // Set the corresponding property in the cookie object.

                    switch (property.ToLowerInvariant()) {

                        case "name":
                            cookie.Name = value;
                            break;

                        case "value":
                            cookie.Value = value;
                            break;

                        case "domain":
                            cookie.Domain = value;
                            break;

                        case "path":
                            cookie.Path = value;
                            break;

                        case "expires":
                            cookie.Expires = DateUtilities.FromUnixTimeSeconds((long)reader.ReadAsDouble().Value).DateTime;
                            break;

                        case "httpOnly":
                            cookie.HttpOnly = reader.ReadAsBoolean().Value;
                            break;

                        case "secure":
                            cookie.HttpOnly = reader.ReadAsBoolean().Value;
                            break;

                    }

                }

            }

            return cookie;

        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            Cookie cookie = value as Cookie;

            writer.WriteStartObject();

            writer.WritePropertyName("name");
            writer.WriteValue(cookie.Name);
            writer.WritePropertyName("value");
            writer.WriteValue(cookie.Value);
            writer.WritePropertyName("domain");
            writer.WriteValue(cookie.Domain);
            writer.WritePropertyName("path");
            writer.WriteValue(cookie.Path);
            writer.WritePropertyName("expires");
            writer.WriteValue(cookie.Expires.ToUnixTimeSeconds());
            writer.WritePropertyName("httpOnly");
            writer.WriteValue(cookie.HttpOnly);
            writer.WritePropertyName("secure");
            writer.WriteValue(cookie.Secure);
            writer.WritePropertyName("sameSite");
            writer.WriteValue("None");

            writer.WriteEndObject();

        }

    }

}