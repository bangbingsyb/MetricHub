using MetricHub.Infrastructure;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;

namespace MetricHub.MetricProvider
{
    public class EtwMetricProvider : MetricProviderBase
    {
        private EtwMonitorOptions _options = new EtwMonitorOptions()
        {
            SessionName = nameof(EtwMetricProvider),
            ProviderGuid = new Guid("3A2A4E84-4C21-4981-AE10-3FDA0D9B0F83"),
            ProviderName = "IIS: WWW Server",
            Flags = 0xFFFFFFFE,
            Level = TraceEventLevel.Verbose
        };

        public override void Start()
        {
            if (!(TraceEventSession.IsElevated() ?? false))
            {
                Console.WriteLine("To turn on ETW events you need to be Administrator, please run from an Admin process.");
                return;
            }

            // Create a TraceEventSession
            using (var session = new TraceEventSession(_options.SessionName))
            {
                // Hook up a callback for every event the Dynamic parser knows about.
                session.Source.Dynamic.All += ParseEvent;

                var restarted = session.EnableProvider(_options.ProviderGuid, _options.Level, _options.Flags);
                session.Source.Process();
            }
        }

        private void ParseEvent(TraceEvent data)
        {
            OnPublishMetric(data.ToString());
        }
    }
}
