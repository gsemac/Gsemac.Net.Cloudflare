﻿using Gsemac.Net.Cloudflare.FlareSolverr.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrResponse {

        [JsonProperty("solution")]
        FlareSolverrSolution Solution { get; }
        [JsonProperty("session")]
        string Session { get; }
        [JsonProperty("sessions")]
        IEnumerable<string> Sessions { get; }
        [JsonProperty("status"), JsonConverter(typeof(FlareSolverrResponseStatusJsonConverter))]
        FlareSolverrResponseStatus Status { get; }
        [JsonProperty("message")]
        string Message { get; }
        [JsonProperty("startTimestamp"), JsonConverter(typeof(FlareSolverrMillisecondsUnixEpochJsonConverter))]
        DateTimeOffset StartTimestamp { get; }
        [JsonProperty("endTimestamp"), JsonConverter(typeof(FlareSolverrMillisecondsUnixEpochJsonConverter))]
        DateTimeOffset EndTimestamp { get; }
        // Version strings are of the form "v1.2.3", which the built-in version converter can't parse
        [JsonProperty("version"), JsonConverter(typeof(FlareSolverrVersionConverter))]
        Version Version { get; }

    }

}