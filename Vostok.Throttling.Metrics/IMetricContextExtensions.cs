using System;
using JetBrains.Annotations;

namespace Vostok.Throttling.Metrics
{
    [PublicAPI]
    public static class IMetricContextExtensions
    {
        [NotNull]
        public static IDisposable CreateThrottlingMetrics([NotNull] IThrottlingProvider provider)
        {
            // TODO(iloktionov): capacity limit
            // TODO(iloktionov): queue limit
            // TODO(iloktionov): max consumed capacity
            // TODO(iloktionov): max capacity utilization
            // TODO(iloktionov): max queue size
            // TODO(iloktionov): max queue utilization
            // TODO(iloktionov): max consumption by property by value
            // TODO(iloktionov): counter by rejection reason
            // TODO(iloktionov): summary by wait time

            throw new NotImplementedException();
        }
    }
}
