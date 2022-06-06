using Gsemac.Core;
using Gsemac.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr.Json {

    internal class FlareSolverrCookieJsonConverter :
        JsonConverter {

        // Public members

        public override bool CanConvert(Type objectType) {

            return objectType.Equals(typeof(Cookie));

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            Cookie cookie = new Cookie();
            JToken token = JToken.Load(reader);

            // I've come across some websites sending cookies where the name is an empty string.
            // Some web browsers will tolerate this (e.g. Google Chrome), but System.Net.Cookie will reject the name and throw an exception.
            // What is considered valid behavior according to the RFC is apparently contradictory: https://stackoverflow.com/a/61695783/5383169 (Buffoonism)

            string cookieName = (string)token["name"];

            if (string.IsNullOrEmpty(cookieName))
                return null;

            cookie.Name = (string)token["name"];

            if (token["value"] != null)
                cookie.Value = (string)token["value"];

            if (token["domain"] != null)
                cookie.Domain = (string)token["domain"];

            if (token["path"] != null)
                cookie.Path = (string)token["path"];

            if (token["expires"] != null)
                cookie.Expires = DateUtilities.FromUnixTimeSeconds((long)(double)token["expires"]).DateTime;

            if (token["httpOnly"] != null)
                cookie.HttpOnly = (bool)token["httpOnly"];

            if (token["secure"] != null)
                cookie.Secure = (bool)token["secure"];

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