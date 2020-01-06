using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Metrics;
using Vostok.Metrics.Grouping;
using Vostok.Metrics.Primitives.Counter;
using Vostok.Metrics.Primitives.Gauge;
using Vostok.Metrics.Primitives.Timer;

namespace Vostok.Throttling.Metrics
{
    internal class ThrottlingMetrics : IDisposable, IObserver<IThrottlingEvent>, IObserver<IThrottlingResult>
    {
        private readonly IDisposable eventSubscription;
        private readonly IDisposable resultSubscription;

        private readonly ITimer waitTimeSummary;
        private readonly IMetricGroup1<ICounter> rejectionCounter;
        private readonly IIntegerGauge maxCapacityLimit;
        private readonly IIntegerGauge maxCapacityConsumed;
        private readonly IIntegerGauge maxQueueLimit;
        private readonly IIntegerGauge maxQueueSize;
        private readonly IFloatingGauge maxCapacityUtilization;
        private readonly IFloatingGauge maxQueueUtilization;

        private readonly Dictionary<string, IMetricGroup1<IIntegerGauge>> maxConsumptionPerProperty;

        public ThrottlingMetrics(
            [NotNull] IThrottlingProvider provider,
            [NotNull] IMetricContext metricContext,
            [NotNull] ThrottlingMetricsOptions options)
        {
            eventSubscription = provider.Subscribe(this as IObserver<IThrottlingEvent>);
            resultSubscription = provider.Subscribe(this as IObserver<IThrottlingResult>);

            waitTimeSummary = metricContext.CreateSummary("queueWaitTime");
            rejectionCounter = metricContext.CreateCounter("rejectionsCount", "status");

            var integerGaugeConfig = new IntegerGaugeConfig {InitialValue = 0, ResetOnScrape = true};
            var floatingGaugeConfig = new FloatingGaugeConfig {InitialValue = 0, ResetOnScrape = true};

            maxCapacityLimit = metricContext.CreateIntegerGauge("maxCapacityLimit", integerGaugeConfig);
            maxCapacityConsumed = metricContext.CreateIntegerGauge("maxCapacityConsumed", integerGaugeConfig);
            maxCapacityUtilization = metricContext.CreateFloatingGauge("maxCapacityUtilization", floatingGaugeConfig);

            maxQueueLimit = metricContext.CreateIntegerGauge("maxQueueLimit", integerGaugeConfig);
            maxQueueSize = metricContext.CreateIntegerGauge("maxQueueSize", integerGaugeConfig);
            maxQueueUtilization = metricContext.CreateFloatingGauge("maxQueueUtilization", floatingGaugeConfig);

            maxConsumptionPerProperty = options.PropertiesWithConsumptionTracking
                .ToDictionary(
                    property => property,
                    property => metricContext
                        .WithTag("scope", "property")
                        .WithTag("propertyName", property)
                        .CreateIntegerGauge("maxConsumption", "propertyValue", integerGaugeConfig));
        }

        public void Dispose()
        {
            eventSubscription.Dispose();
            resultSubscription.Dispose();

            (waitTimeSummary as IDisposable)?.Dispose();
            (rejectionCounter as IDisposable)?.Dispose();

            (maxCapacityLimit as IDisposable)?.Dispose();
            (maxCapacityConsumed as IDisposable)?.Dispose();
            (maxCapacityUtilization as IDisposable)?.Dispose();

            (maxQueueLimit as IDisposable)?.Dispose();
            (maxQueueSize as IDisposable)?.Dispose();
            (maxQueueUtilization as IDisposable)?.Dispose();

            foreach (var gauge in maxConsumptionPerProperty.Values)
                (gauge as IDisposable)?.Dispose();
        }

        public void OnNext(IThrottlingEvent evt)
        {
            maxCapacityLimit.TryIncreaseTo(evt.CapacityLimit);
            maxCapacityConsumed.TryIncreaseTo(evt.CapacityConsumed);
            maxCapacityUtilization.TryIncreaseTo(ComputeUtilization(evt.CapacityConsumed, evt.CapacityLimit));

            maxQueueLimit.TryIncreaseTo(evt.QueueLimit);
            maxQueueSize.TryIncreaseTo(evt.QueueSize);
            maxQueueUtilization.TryIncreaseTo(ComputeUtilization(evt.QueueSize, evt.QueueLimit));

            foreach (var pair in maxConsumptionPerProperty)
            {
                if (evt.Properties.TryGetValue(pair.Key, out var propertyValue) &&
                    evt.PropertyConsumption.TryGetValue(pair.Key, out var consumption))
                {
                    pair.Value.For(propertyValue).TryIncreaseTo(consumption);
                }
            }
        }

        public void OnNext(IThrottlingResult result)
        {
            if (result.Status != ThrottlingStatus.Passed)
                rejectionCounter.For(result.Status).Increment();

            waitTimeSummary.Report(result.WaitTime);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        private static double ComputeUtilization(int consumed, int limit)
            => (double)consumed / Math.Max(1, limit);
    }
}
