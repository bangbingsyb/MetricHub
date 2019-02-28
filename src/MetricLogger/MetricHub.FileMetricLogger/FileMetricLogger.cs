using MetricHub.Infrastructure;
using System;
using System.IO;

namespace MetricHub.MetricLogger
{
    public class FileMetricLogger : IMetricLogger
    {
        private static string _logFilePath = "log.txt";

        public void Log(object sender, string data)
        {
            using (StreamWriter w = File.AppendText(_logFilePath))
            {
                w.WriteLine(data);
            }
        }
    }
}
