using MetricHub.Infrastructure;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MetricHub.MetricProvider
{
    public class EtwMetricProvider : MetricProviderBase
    {
        private string _sessionName;

        private List<EtwProviderDefinition> _etwProviderList = new List<EtwProviderDefinition>();

        public override void Start()
        {
            if (!(TraceEventSession.IsElevated() ?? false))
            {
                Console.WriteLine("To turn on ETW events you need to be Administrator, please run from an Admin process.");
                return;
            }

            Console.WriteLine($"Starting {nameof(EtwMetricProvider)} ...");

            Initialize();

            // Create a TraceEventSession
            using (var session = new TraceEventSession(_sessionName))
            {
                Console.WriteLine($"Created the ETW session {_sessionName}.");

                // Hook up a callback for every event the Dynamic parser knows about.
                session.Source.Dynamic.All += ParseEvent;

                foreach (var provider in _etwProviderList)
                {
                    if (string.IsNullOrEmpty(provider.ProviderGuid.ToString()))
                    {
                        if (string.IsNullOrEmpty(provider.ProviderName))
                        {
                            throw new ArgumentException();
                        }
                        var restarted = session.EnableProvider(provider.ProviderName, provider.Level, provider.Flags);
                    }
                    else
                    {
                        var restarted = session.EnableProvider(new Guid(provider.ProviderGuid), provider.Level, provider.Flags);
                    }
                }

                session.Source.Process();
            }
        }

        private void Initialize()
        {
            if (string.IsNullOrEmpty(_sessionName))
            {
                _sessionName = nameof(EtwMetricProvider) + "-" + Guid.NewGuid().ToString();
            }

            string configFilePath = Utils.GetAssemblyDirectory() + Path.DirectorySeparatorChar + this.GetType().Name + ".json";
            string json = File.ReadAllText(configFilePath);

            _etwProviderList = JsonConvert.DeserializeObject<List<EtwProviderDefinition>>(json);

            foreach (var provider in _etwProviderList)
            {
                Console.WriteLine($"ProviderName: {provider.ProviderName}, " +
                                  $"ProviderGuid: {provider.ProviderGuid}, " +
                                  $"Flags: {provider.Flags.ToString()}, " +
                                  $"Level: {provider.Level.ToString()}");
            }
        }

        private void ParseEvent(TraceEvent data)
        {
            OnPublishMetric(data.ToString());
        }
    }
}
