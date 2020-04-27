using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebAppHostingDotNetInteractive {
    public class DotNetInteractiveWorkerHost : IHostedService {
        private readonly ILogger<DotNetInteractiveWorkerHost> logger;
        public DotNetInteractiveWorkerHost (ILogger<DotNetInteractiveWorkerHost> logger) {
            this.logger = logger;
        }

        public Process DotNetInteractiveProcess { get; private set; }

        public Task StartAsync (CancellationToken cancellationToken) {
            logger.LogInformation ("Starting up.");

            var startInfo = new ProcessStartInfo ("dotnet", "interactive stdio --default-kernel csharp") 
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            DotNetInteractiveProcess = new Process {
                StartInfo = startInfo
            };

            DotNetInteractiveProcess.Start ();

            return Task.CompletedTask;
        }

        public Task StopAsync (CancellationToken cancellationToken) {
            logger.LogInformation("Stopping.");
            DotNetInteractiveProcess.Kill();

            return Task.CompletedTask;
        }
    }
}