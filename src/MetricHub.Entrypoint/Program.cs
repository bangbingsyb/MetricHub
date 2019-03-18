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
            //IisConfigHelper.UpdateEnvironmentVariables();
            IisMonitor.Monitor();
        }

        static void CollectMetrics()
        {
            IMetricLogger consoleLogger = new ConsoleMetricLogger();
            //IMetricLogger fileLogger = new FileMetricLogger();

            IMetricProvider perfCounterProvider = new PerfCounterMetricProvider();
            perfCounterProvider.Subscribe(consoleLogger);
            Task.Factory.StartNew(() => { perfCounterProvider.Start(); });

            IMetricProvider etwProvider = new EtwMetricProvider();
            etwProvider.Subscribe(consoleLogger);
            Task.Factory.StartNew(() => { etwProvider.Start(); });

            IMetricProvider logFileProvider = new LogFileMetricProvider();
            logFileProvider.Subscribe(consoleLogger);
            Task.Factory.StartNew(() => { logFileProvider.Start(); });
        }
    }
}
