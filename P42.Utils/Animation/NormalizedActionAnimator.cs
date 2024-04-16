using System;
using System.Threading.Tasks;

namespace P42.Utils
{
    public class NormalizedActionAnimator
    {
        public TimeSpan TimeSpan { get; private set; }

        public Easing EasingFunction { get; private set; }

        public Action<double> Action { get; private set; }

        protected DateTime StartTime;

        public NormalizedActionAnimator(TimeSpan timeSpan, Action<double> action, Easing easingFunction = null)
        {
            TimeSpan = timeSpan;
            EasingFunction = easingFunction;
            Action = action;
        }

        public async Task RunAsync()
        {
            StartTime = DateTime.Now;
            double normalTime;
            do
            {
                await Task.Delay(20);
                normalTime = Math.Min((DateTime.Now - StartTime).TotalMilliseconds / TimeSpan.TotalMilliseconds, 1.0);
                var normalValue = EasingFunction?.Ease(normalTime) ?? normalTime;
                var value = Value(normalValue);
                this?.Action(value);
            }
            while (normalTime < 1.0);
        }

        protected virtual double Value(double normalValue)
        {
            return normalValue;
        }
    }
}
