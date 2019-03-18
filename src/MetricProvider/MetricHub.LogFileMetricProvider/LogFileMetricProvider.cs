using MetricHub.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MetricHub.MetricProvider
{
    public class LogFileMetricProvider : MetricProviderBase
    {
        private const string tagName = "Windows.LogFile";

        private List<LogFileMonitor> _monitorList;

        public LogFileMetricProvider()
        {
            Tag = tagName;

            _monitorList = new List<LogFileMonitor>();
        }

        public override void Start()
        {
            Initialize();

            foreach (var monitor in _monitorList)
            {
                Task.Factory.StartNew(() => { monitor.Start(); });
            }
        }

        public override void Stop()
        {
            foreach (var monitor in _monitorList)
            {
                monitor.Stop();
            }

            bool fStopped = false;
            while (fStopped == false)
            {
                fStopped = true;
                foreach (var monitor in _monitorList)
                {
                    if (monitor.IsStopped == false)
                    {
                        fStopped = false;
                        Thread.Sleep(TimeSpan.FromMilliseconds(1000));
                        continue;
                    }
                }
            }
        }

        private void Initialize()
        {
            string configFilePath = Utils.GetAssemblyDirectory() + Path.DirectorySeparatorChar + this.GetType().Name + ".json";
            string json = File.ReadAllText(configFilePath);

            List<LogFileMonitorDefinition>  definitionList = JsonConvert.DeserializeObject<List<LogFileMonitorDefinition>>(json);

            foreach (var defintion in definitionList)
            {
                Console.WriteLine($"Creating log file monitor - Directory: {defintion.DirectoryPath}, Filter: {defintion.Filter}");
                var logFileMonitor = new LogFileMonitor(defintion.DirectoryPath, defintion.Filter, defintion.IncludeSubdirectories);
                logFileMonitor.LogHandler += Log;
                _monitorList.Add(logFileMonitor);
            }
        }

        private void Log(object sender, string data)
        {
            using (StringReader sr = new StringReader(data))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    OnPublishMetric(line);
                }
            }
        }
    }
}
