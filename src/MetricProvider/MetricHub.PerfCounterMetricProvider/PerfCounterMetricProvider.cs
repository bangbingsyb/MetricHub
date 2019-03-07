using MetricHub.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MetricHub.MetricProvider
{
    public class PerfCounterMetricProvider : MetricProviderBase
    {
        private const string tagName = "Windows.PerfCounter";

        private List<PerformanceCounter> _perfCounterList = new List<PerformanceCounter>();

        public PerfCounterMetricProvider()
        {
            Tag = tagName;
        }

        public override void Start()
        {
            Initialize();

            foreach (var perfCounter in _perfCounterList)
            {
                Task.Factory.StartNew(() => { MonitorPerfCounter(perfCounter); });
            }
        }

        private void MonitorPerfCounter(PerformanceCounter perfCounter)
        {
            perfCounter.NextValue();

            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));
                string data = $"{perfCounter.CategoryName}\\{perfCounter.CounterName}\\{perfCounter.InstanceName}: {perfCounter.NextValue().ToString()}";
                OnPublishMetric(data);
            }
        }

        private void Initialize()
        {
            string configFilePath = Utils.GetAssemblyDirectory() + Path.DirectorySeparatorChar + this.GetType().Name + ".json";
            string json = File.ReadAllText(configFilePath);

            List<PerfCounterDefinition> perfCounterDefinitions = JsonConvert.DeserializeObject<List<PerfCounterDefinition>>(json);

            foreach (var def in perfCounterDefinitions)
            {
                Console.WriteLine($"Initializing performance counter: {def.CategoryName}\\{def.CounterName}\\{def.InstanceName}");
                _perfCounterList.Add(new PerformanceCounter(def.CategoryName, def.CounterName, def.InstanceName, true));
            }
        }
    }
}
