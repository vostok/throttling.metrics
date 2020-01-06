using System;
using JetBrains.Annotations;
using Vostok.Metrics;

namespace Vostok.Throttling.Metrics
{
    [PublicAPI]
    public static class IMetricContextExtensions
    {
        [NotNull]
        public static IDisposable CreateThrottlingMetrics(
            [NotNull] this IMetricContext context,
            [NotNull] IThrottlingProvider provider,
            [CanBeNull] ThrottlingMetricsOptions options = null)
            => new ThrottlingMetrics(provider, context, options ?? new ThrottlingMetricsOptions());
    }
}
