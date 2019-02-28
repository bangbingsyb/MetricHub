using System;
using System.Collections.Generic;
using System.Text;

namespace MetricHub.Infrastructure
{
    public abstract class MetricProviderBase : IMetricProvider
    {
        public event EventHandler<string> MetricPublisher;

        public abstract void Start();

        public void Subscribe(IMetricLogger logger)
        {
            MetricPublisher += logger.Log;
        }

        protected void OnPublishMetric(string data)
        {
            MetricPublisher?.Invoke(this, data);
        }
    }
}
