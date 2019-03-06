using System;

namespace MetricHub.Infrastructure
{
    public abstract class MetricProviderBase : IMetricProvider
    {
        public string Tag { get; set; }

        public event EventHandler<string> MetricPublisher;

        public abstract void Start();

        public void Subscribe(IMetricLogger logger)
        {
            MetricPublisher += logger.Log;
        }

        protected void OnPublishMetric(string data)
        {
            var record = new MetricRecord()
            {
                Tag = Tag,
                Data = data
            };

            MetricPublisher?.Invoke(this, record.ToString());
        }
    }
}
