﻿using System;
using System.Net;

namespace Gsemac.Net.Cloudflare {

    public abstract class IuamChallengeSolverOptionsBase :
        IIuamChallengeSolverOptions {

        public IWebProxy Proxy { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public string UserAgent { get; set; }

    }

}