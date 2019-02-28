using MetricHub.Infrastructure;
using System;

namespace MetricHub.MetricLogger
{
    public class ConsoleMetricLogger : IMetricLogger
    {
        public void Log(object sender, string data)
        {
            Console.WriteLine(data);
        }
    }
}
