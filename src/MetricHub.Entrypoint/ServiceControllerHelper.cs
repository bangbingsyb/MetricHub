using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetricHub.Entrypoint
{
    public static class ServiceControllerHelper
    {
        public static void StartService(string serviceName, TimeSpan timeout)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    try
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine($"Could not start the {serviceName} service. The service is in {sc.Status.ToString()} status.");
                    }
                    finally
                    {
                        Console.WriteLine($"The {serviceName} service is in {sc.Status.ToString()} status.");
                        sc.Close();
                    }
                }
            }
        }

        public static void StopService(string serviceName, TimeSpan timeout)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    try
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine($"Could not stop the {serviceName} service.");
                    }
                    finally
                    {
                        Console.WriteLine($"The {serviceName} service is in {sc.Status.ToString()} status.");
                        sc.Close();
                    }
                }
            }
        }

        public static void MonitorService(string serviceName, List<ServiceControllerStatus> statusList, CancellationToken cancellationToken)
        {
            using (var sc = new ServiceController(serviceName))
            {
                Console.WriteLine($"Start to monitor the {serviceName} service.");

                try
                {
                    sc.Refresh();

                    while (!cancellationToken.IsCancellationRequested && !statusList.Contains(sc.Status))
                    {
                        Thread.Sleep(250);
                        sc.Refresh();
                    }
                }
                finally
                {
                    Console.WriteLine($"Stop to monitor the {serviceName} service.");
                    sc.Close();
                }
            }
        }
    }
}
