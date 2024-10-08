﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public sealed class FlareSolverrRequest :
        IFlareSolverrRequest {

        public const string CreateSession = "sessions.create";
        public const string DestroySession = "sessions.destroy";
        public const string GetRequest = "request.get";
        public const string PostRequest = "request.post";

        public string Command { get; set; }
        public bool Download { get; set; } = false;
        public Uri Url { get; set; }
        public string UserAgent { get; set; }
        public TimeSpan MaxTimeout { get; set; } = TimeSpan.FromSeconds(60);
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public CookieCollection Cookies { get; } = new CookieCollection();
        public string Session { get; set; }
        public bool ReturnOnlyCookies { get; set; } = false;
        public string PostData { get; set; }

        public FlareSolverrRequest(string command) {
            Command = command;
        }

        public override string ToString() {

            return JsonConvert.SerializeObject(this, new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            });

        }

    }

}