using Gsemac.Core;
using Gsemac.Core.Extensions;
using Newtonsoft.Json;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr.Json {

    internal class FlareSolverrDataVersionConverter :
        JsonConverter {

        public override bool CanConvert(Type objectType) {

            return typeof(IVersion).IsAssignableFrom(objectType) || objectType.Equals(typeof(System.Version));

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            string versionString = (string)reader.Value;

            if (string.IsNullOrEmpty(versionString))
                return new System.Version(0, 0);

            return Core.Version.Parse(versionString).ToVersion();

        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            writer.WriteValue($"v{value}");

        }

    }

}