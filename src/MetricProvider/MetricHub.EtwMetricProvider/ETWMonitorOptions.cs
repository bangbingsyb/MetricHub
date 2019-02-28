using Microsoft.Diagnostics.Tracing;
using System;

namespace MetricHub.MetricProvider
{
    public class EtwMonitorOptions
    {
        public string SessionName { get; set; }

        public string ProviderName { get; set; }

        public Guid ProviderGuid { get; set; }

        public ulong Flags { get; set; }

        public TraceEventLevel Level { get; set; }
    }
}
