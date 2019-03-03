using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetricHub.Entrypoint
{
    public static class IisMonitor
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public static void Monitor()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ControlSignalHandler);

            string serviceName = "w3svc";
            var statusList = new List<ServiceControllerStatus>()
            {
                { ServiceControllerStatus.Stopped },
                { ServiceControllerStatus.StopPending },
                { ServiceControllerStatus.Paused },
                { ServiceControllerStatus.PausePending },
            };

            ServiceControllerHelper.StopService(serviceName, TimeSpan.FromSeconds(120));

            // set console handler

            ServiceControllerHelper.StartService(serviceName, TimeSpan.FromSeconds(120));

            ServiceControllerHelper.MonitorService(serviceName, statusList, _cts.Token);

            ServiceControllerHelper.StopService(serviceName, TimeSpan.FromSeconds(120));
        }

        private static void ControlSignalHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("CTRL signal received. The process will now terminate.");

            _cts.Cancel();
            args.Cancel = true;
        }
    }
}
