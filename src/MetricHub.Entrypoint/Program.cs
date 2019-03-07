using MetricHub.Infrastructure;
using MetricHub.MetricLogger;
using MetricHub.MetricProvider;
using System;
using System.Threading.Tasks;

namespace MetricHub.Entrypoint
{
    class Program
    {
        static void Main(string[] args)
        {
            CollectMetrics();
            IisConfigHelper.UpdateEnvironmentVariables();
            IisMonitor.Monitor();
        }

        static void CollectMetrics()
        {
            IMetricLogger consoleLogger = new ConsoleMetricLogger();
            //IMetricLogger fileLogger = new FileMetricLogger();

            IMetricProvider perfCounterProvider = new PerfCounterMetricProvider();
            IMetricProvider etwProvider = new EtwMetricProvider();

            //perfCounterProvider.Subscribe(fileLogger);
            perfCounterProvider.Subscribe(consoleLogger);
            etwProvider.Subscribe(consoleLogger);

            Task.Factory.StartNew(() => { perfCounterProvider.Start(); });
            Task.Factory.StartNew(() => { etwProvider.Start(); });
        }
    }
}
