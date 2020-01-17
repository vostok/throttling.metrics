using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Throttling.Metrics
{
    [PublicAPI]
    public class ThrottlingMetricsOptions
    {
        /// <summary>
        /// An optional list of properties with per-value consumption metrics tracking. 
        /// </summary>
        [NotNull]
        public List<string> PropertiesWithConsumptionTracking = new List<string>
        {
            WellKnownThrottlingProperties.Consumer
        };
    }
}