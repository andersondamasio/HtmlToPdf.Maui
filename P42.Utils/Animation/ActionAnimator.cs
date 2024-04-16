using System;
namespace P42.Utils
{
    public class ActionAnimator : NormalizedActionAnimator
    {
        public double From { get; private set; }

        public double To { get; private set; }



        public ActionAnimator(double from, double to, TimeSpan timeSpan, Action<double> action, Easing easingFunction = null)
            : base(timeSpan, action, easingFunction)
        {
            From = from;
            To = to;
        }

        protected override double Value(double normalizedValue)
        {
            var value = From + normalizedValue * (To - From);
            return value;
        }
    }
}
