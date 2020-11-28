using System;
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

        /// <summary>
        /// Period of scraping throttling metrics. If left <c>null</c>, context default period will be used.
        /// </summary>
        [CanBeNull]
        public TimeSpan? ScrapePeriod { get; set; }
    }
}