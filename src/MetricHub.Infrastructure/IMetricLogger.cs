using System;
using System.Collections.Generic;
using System.Text;

namespace MetricHub.Infrastructure
{
    public interface IMetricLogger
    {
        void Log(object sender, string data);

    }
}
