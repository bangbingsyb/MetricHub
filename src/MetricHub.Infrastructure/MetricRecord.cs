using Newtonsoft.Json;

namespace MetricHub.Infrastructure
{
    public class MetricRecord
    {
        public string Tag { get; set; }

        public string Data { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
