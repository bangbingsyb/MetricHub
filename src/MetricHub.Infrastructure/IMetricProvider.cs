using System;
using System.Collections.Generic;
using System.Text;

namespace MetricHub.Infrastructure
{
    public interface IMetricProvider
    {
        event EventHandler<string> MetricPublisher;

        void Start();

        void Subscribe(IMetricLogger logger);
    }
}
