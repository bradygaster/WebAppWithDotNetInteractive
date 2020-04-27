using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebAppHostingDotNetInteractive {
    public class DotNetInteractiveWorkerHost : BackgroundService {
        private readonly ILogger<DotNetInteractiveWorkerHost> logger;
        public DotNetInteractiveWorkerHost (ILogger<DotNetInteractiveWorkerHost> logger) {
            this.logger = logger;
        }

        public Process DotNetInteractiveProcess { get; private set; }

        public override Task StartAsync (CancellationToken cancellationToken) {
            logger.LogInformation ("Starting up.");

            base.StartAsync(cancellationToken);

            var startInfo = new ProcessStartInfo ("dotnet", "interactive stdio --default-kernel csharp") 
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            DotNetInteractiveProcess = new Process {
                EnableRaisingEvents = true,
                StartInfo = startInfo
            };

            DotNetInteractiveProcess.ErrorDataReceived += (s, e) => logger.LogError(e.Data);
            DotNetInteractiveProcess.OutputDataReceived += (s, e) => logger.LogInformation(e.Data);
            DotNetInteractiveProcess.Exited += (s, e) => logger.LogCritical(".NET Interactive Exited");
            DotNetInteractiveProcess.Start ();

            return Task.CompletedTask;
        }

        public override Task StopAsync (CancellationToken cancellationToken) {
            base.StopAsync(cancellationToken);
            logger.LogInformation("Stopping.");
            DotNetInteractiveProcess.Kill();

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10000);
                logger.LogDebug("Worker with host is running, sending sample to myself.");

                await DotNetInteractiveProcess.StandardInput.WriteLineAsync(
                    "{\"token\":\"1\",\"commandType\":\"SubmitCode\",\"command\":{\"code\": \"var a = 1 + 13;\"}}"
                );
            }
        }
    }
}