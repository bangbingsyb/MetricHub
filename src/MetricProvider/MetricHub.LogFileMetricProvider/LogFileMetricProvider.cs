using MetricHub.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MetricHub.MetricProvider
{
    public class LogFileMetricProvider : MetricProviderBase
    {
        private const string tagName = "Windows.LogFile";

        private List<LogFileMonitorDefinition> _definitionList;
        private List<LogFileMonitor> _monitorList;

        public LogFileMetricProvider()
        {
            Tag = tagName;

            _definitionList = new List<LogFileMonitorDefinition>();
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

        private void Initialize()
        {
            string configFilePath = Utils.GetAssemblyDirectory() + Path.DirectorySeparatorChar + this.GetType().Name + ".json";
            string json = File.ReadAllText(configFilePath);

            _definitionList = JsonConvert.DeserializeObject<List<LogFileMonitorDefinition>>(json);

            foreach (var defintion in _definitionList)
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
