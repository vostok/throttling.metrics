using System;
using JetBrains.Annotations;
using Vostok.Metrics;

namespace Vostok.Throttling.Metrics
{
    [PublicAPI]
    public static class IMetricContextExtensions
    {
        /// <summary>
        /// <para>Registers and tracks following metrics about given <see cref="IThrottlingProvider"/> instance:</para>
        /// <list type="bullet">
        ///     <item><description>Max capacity limit.</description></item>
        ///     <item><description>Max capacity consumption (total).</description></item>
        ///     <item><description>Max capacity consumption (per property). See <see cref="ThrottlingMetricsOptions"/> for more details.</description></item>
        ///     <item><description>Max capacity utilization.</description></item>
        ///     <item><description>Max queue limit.</description></item>
        ///     <item><description>Max queue size.</description></item>
        ///     <item><description>Max queue utilization.</description></item>
        ///     <item><description>Rejection status counters.</description></item>
        ///     <item><description>Queue wait time timings.</description></item>
        /// </list>
        /// <para>Adds a '<c>component = throttling</c>' tag to the provided metric <paramref name="context"/> before registering metrics.</para>
        /// <para>Dispose of the returned result to stop metrics collection.</para>
        /// </summary>
        [NotNull]
        public static IDisposable CreateThrottlingMetrics(
            [NotNull] this IMetricContext context,
            [NotNull] IThrottlingProvider provider,
            [CanBeNull] ThrottlingMetricsOptions options = null)
            => new ThrottlingMetrics(provider, context.WithTag(WellKnownTagKeys.Component, "throttling"), options ?? new ThrottlingMetricsOptions());
    }
}
