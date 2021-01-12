﻿using Gsemac.Core;
using Gsemac.IO.Logging;
using Gsemac.IO.Logging.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrService :
        FlareSolverrServiceBase {

        // Public members

        public FlareSolverrService(IFlareSolverrOptions options, ILogger logger = null) {

            this.options = options;

            if (logger is object)
                Log += logger.CreateLogEventHandler();

        }

        public override bool Start() {

            lock (mutex) {

                if (!processStarted) {

                    OnLog.Info("Starting FlareSolverr service");

                    processStarted = StartFlareSolverr();

                    if (!processStarted)
                        OnLog.Error(" Failed to start FlareSolverr process");

                }

            }

            return processStarted;

        }
        public override bool Stop() {

            lock (mutex) {

                if (processStarted) {

                    OnLog.Info("Stopping FlareSolverr service");

                    StopFlareSolverr();

                    processStarted = false;

                }

            }

            return processStarted;

        }

        // Protected members

        protected override void Dispose(bool disposing) {

            if (disposing) {

                lock (mutex)
                    StopFlareSolverr();

            }

        }

        // Private members

        private readonly object mutex = new object();
        private bool processStarted = false;
        private readonly IFlareSolverrOptions options;
        private Process flareSolverrProcess;

        private bool StartFlareSolverr() {

            if (!File.Exists(options.FlareSolverrExecutablePath)) {

                OnLog.Error($"FlareSolverr was not found at '{options.FlareSolverrExecutablePath}'");

                throw new FileNotFoundException(options.FlareSolverrExecutablePath);

            }

            OnLog.Info($"Starting FlareSolverr process");

            flareSolverrProcess = CreateProcess(options.FlareSolverrExecutablePath);

            bool success = flareSolverrProcess.Start();

            flareSolverrProcess.BeginOutputReadLine();
            flareSolverrProcess.BeginErrorReadLine();

            // Give the process some time to fail so we can detect if FlareSolverr failed to start.

            if (success) {

                if (flareSolverrProcess.WaitForExit((int)TimeSpan.FromSeconds(1).TotalMilliseconds))
                    success = success && flareSolverrProcess.ExitCode == 0;

            }

            if (success)
                OnLog.Info($"FlareSolverr is now listening on port {FlareSolverrUtilities.DefaultPort}");

            return success;

        }
        private void StopFlareSolverr() {

            if (processStarted && flareSolverrProcess != null && !flareSolverrProcess.HasExited) {

                OnLog.Info("Stopping FlareSolverr process");

                flareSolverrProcess.Kill();
                flareSolverrProcess.Dispose();

                flareSolverrProcess = null;

            }

        }

        private Process CreateProcess(string fileName) {

            ProcessStartInfo processStartInfo = new ProcessStartInfo() {
                FileName = fileName,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            Process process = new Process {
                StartInfo = processStartInfo
            };

            process.OutputDataReceived += (sender, e) => OnLog.Info(e.Data);
            process.ErrorDataReceived += (sender, e) => OnLog.Error(e.Data);

            return process;

        }

    }

}