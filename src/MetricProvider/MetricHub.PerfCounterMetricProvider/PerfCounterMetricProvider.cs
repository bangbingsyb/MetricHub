using MetricHub.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetricHub.MetricProvider
{
    public class PerfCounterMetricProvider : MetricProviderBase
    {
        public override void Start()
        {
            string categoryName = "Process";
            string counterName = "% Processor Time";
            string instanceName = "_Total";
            string prefix = $"{categoryName}\\{counterName}\\{instanceName}: ";

            PerformanceCounter perfCounter = new PerformanceCounter(categoryName, counterName, instanceName);
            while (true)
            {
                Thread.Sleep(1000);
                OnPublishMetric(prefix + perfCounter.NextValue().ToString());
            }
        }
    }
}
