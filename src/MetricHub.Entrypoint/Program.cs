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
            IMetricLogger consoleLogger = new ConsoleMetricLogger();
            IMetricLogger fileLogger = new FileMetricLogger();

            //IMetricProvider etwProvider = new EtwMetricProvider();
            IMetricProvider perfCounterProvider = new PerfCounterMetricProvider();

            perfCounterProvider.Subscribe(fileLogger);
            perfCounterProvider.Subscribe(consoleLogger);

            Task.Factory.StartNew(() => { perfCounterProvider.Start(); });
            //Task.Factory.StartNew(() => { etwProvider.Start(); });

            Console.ReadLine();
        }
    }
}
