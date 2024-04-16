using System;
namespace P42.Utils
{
    public class Easing
    {

        static LinearFunction _linear;
        public static LinearFunction Linear => _linear = _linear ?? new LinearFunction();

        static QuadraticInFunction _quadIn;
        public static QuadraticInFunction QuadIn => _quadIn = _quadIn ?? new QuadraticInFunction();

        static QuadraticOutFunction _quadOut;
        public static QuadraticOutFunction QuadOut => _quadOut = _quadOut ?? new QuadraticOutFunction();

        static QuadraticInOutFunction _quadInOut;
        public static QuadraticInOutFunction QuadInOut => _quadInOut = _quadInOut ?? new QuadraticInOutFunction();

        static CubicInFunction _cubicIn;
        public static CubicInFunction CubicIn => _cubicIn = _cubicIn ?? new CubicInFunction();

        static CubicOutFunction _cubicOut;
        public static CubicOutFunction CubicOut => _cubicOut = _cubicOut ?? new CubicOutFunction();

        static CubicInOutFunction _cubicInOut;
        public static CubicInOutFunction CubicInOut => _cubicInOut = _cubicInOut ?? new CubicInOutFunction();

        static QuarticInFunction _quarticIn;
        public static QuarticInFunction QuarticIn => _quarticIn = _quarticIn ?? new QuarticInFunction();

        static QuarticOutFunction _quarticOut;
        public static QuarticOutFunction QuarticOut => _quarticOut = _quarticOut ?? new QuarticOutFunction();

        static QuarticInOutFunction _quaticInOut;
        public static QuarticInOutFunction QuarticInOut => _quaticInOut = _quaticInOut ?? new QuarticInOutFunction();

        static QuinticInFunction _quinticIn;
        public static QuinticInFunction QuinticIn => _quinticIn = _quinticIn ?? new QuinticInFunction();

        static QuinticOutFunction _quinticOut;
        public static QuinticOutFunction QuinticOut => _quinticOut = _quinticOut ?? new QuinticOutFunction();

        static QuinticInOutFunction _quinticInOut;
        public static QuinticInOutFunction QuinticInOut => _quinticInOut = _quinticInOut ?? new QuinticInOutFunction();

        static SinusoidalInFunction _sinIn;
        public static SinusoidalInFunction SineIn => _sinIn = _sinIn ?? new SinusoidalInFunction();

        static SinusoidalOutFunction _sinOut;
        public static SinusoidalOutFunction SineOut => _sinOut= _sinOut ?? new SinusoidalOutFunction();

        static SinusoidalInOutFunction _sinInOut;
        public static SinusoidalInOutFunction SineInOut => _sinInOut = _sinInOut ?? new SinusoidalInOutFunction();

        static ExponentialInFunction _expIn;
        public static ExponentialInFunction ExponentialIn => _expIn = _expIn ?? new ExponentialInFunction();

        static ExponentialOutFunction _expOut;
        public static ExponentialOutFunction ExponentialOut => _expOut = _expOut ?? new ExponentialOutFunction();

        static ExponentialInOutFunction _expInOut;
        public static ExponentialInOutFunction ExponentialInOut => _expInOut = _expInOut ?? new ExponentialInOutFunction();

        static CircularInFunction _circleIn;
        public static CircularInFunction CircularIn => _circleIn = _circleIn ?? new CircularInFunction();

        static CircularOutFunction _circleOut;
        public static CircularOutFunction CircularOut => _circleOut = _circleOut ?? new CircularOutFunction();

        static CircularInOutFunction _circleInOut;
        public static CircularInOutFunction CircularInOut => _circleInOut = _circleInOut ?? new CircularInOutFunction();

        static ElasticInFunction _elasticIn;
        public static ElasticInFunction ElasticIn => _elasticIn = _elasticIn ?? new ElasticInFunction();

        static ElasticOutFunction _elasticOut;
        public static ElasticOutFunction ElasticOut => _elasticOut = _elasticOut ?? new ElasticOutFunction();

        static ElasticInOutFunction _elasticInOut;
        public static ElasticInOutFunction ElasticInOut => _elasticInOut = _elasticInOut ?? new ElasticInOutFunction();

        static BackInFunction _backIn;
        public static BackInFunction BackIn => _backIn = _backIn ?? new BackInFunction();

        static BackOutFunction _backOut;
        public static BackOutFunction BackOut => _backOut = _backOut ?? new BackOutFunction();

        static BackInOutFunction _backInOut;
        public static BackInOutFunction BackInOut => _backInOut = _backInOut ?? new BackInOutFunction();

        static BounceInFunction _bounceIn;
        public static BounceInFunction BounceIn => _bounceIn = _bounceIn ?? new BounceInFunction();

        static BounceOutFunction _bounceOut;
        public static BounceOutFunction BounceOut => _bounceOut = _bounceOut ?? new BounceOutFunction();

        static BounceInOutFunction _boundInOut;
        public static BounceInOutFunction BounceInOut => _boundInOut = _boundInOut ?? new BounceInOutFunction();

        protected const double s = 1.70158;
        protected const double s2 = 2.5949095;

        public virtual double Ease(double k)
            => throw new NotImplementedException();

    }

    public class LinearFunction : Easing
    {
        public override double Ease(double k)
            => k;
    }

    public class QuadraticInFunction : Easing
    {
        public override double Ease(double k)
            => k * k;
    }

    public class QuadraticOutFunction : Easing
    {
        public override double Ease(double k)
            => k * (2 - k);
    }

    public class QuadraticInOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if ((k *= 2) < 1) return 0.5 * k * k;
            return -0.5 * ((k -= 1) * (k - 2) - 1);
        }
    }

    public class CubicInFunction : Easing
    {
        public override double Ease(double k)
            => k * k * k;
    }

    public class CubicOutFunction : Easing
    {
        public override double Ease(double k)
            => 1 + ((k -= 1) * k * k);
    }

    public class CubicInOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if ((k *= 2) < 1) return 0.5 * k * k * k;
            return 0.5 * ((k -= 2) * k * k + 2);
        }
    }

    public class QuarticInFunction : Easing
    {
        public override double Ease(double k)
            => k * k * k * k;
    }

    public class QuarticOutFunction : Easing
    {
        public override double Ease(double k)
            => 1 - ((k -= 1) * k * k * k);
    }

    public class QuarticInOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if ((k *= 2) < 1) return 0.5 * k * k * k * k;
            return -0.5 * ((k -= 2) * k * k * k - 2);
        }
    }

    public class QuinticOutFunction : Easing
    {
        public override double Ease(double k)
            => k * k * k * k * k;
    }

    public class QuinticInOutFunction : Easing
    {
        public override double Ease(double k)
            => 1 + ((k -= 1) * k * k * k * k);
    }

    public class QuinticInFunction : Easing
    {
        public override double Ease(double k)
        {
            if ((k *= 2) < 1) return 0.5 * k * k * k * k * k;
            return 0.5 * ((k -= 2) * k * k * k * k + 2);
        }
    }

    public class SinusoidalInFunction : Easing
    {
        public override double Ease(double k)
            => 1 - Math.Cos(k * Math.PI / 2);
    }

    public class SinusoidalOutFunction : Easing
    {
        public override double Ease(double k)
            => Math.Sin(k * Math.PI / 2);
    }
    public class SinusoidalInOutFunction : Easing
    {
        public override double Ease(double k)
            => 0.5 * (1 - Math.Cos(Math.PI * k));
    }

    public class ExponentialInFunction : Easing
    {
        public override double Ease(double k)
            => k == 0 ? 0 : Math.Pow(1024, k - 1);
    }

    public class ExponentialOutFunction : Easing
    {
        public override double Ease(double k)
            => k == 1 ? 1 : 1 - Math.Pow(2, -10 * k);
    }

    public class ExponentialInOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if (k == 0) return 0;
            if (k == 1) return 1;
            if ((k *= 2) < 1) return 0.5 * Math.Pow(1024, k - 1);
            return 0.5 * (-Math.Pow(2, -10 * (k - 1)) + 2);
        }
    }

    public class CircularInFunction : Easing
    {
        public override double Ease(double k)
            => 1 - Math.Sqrt(1 - k * k);
    }

    public class CircularOutFunction : Easing
    {
        public override double Ease(double k)
            => Math.Sqrt(1 - ((k -= 1) * k));
    }

    public class CircularInOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if ((k *= 2) < 1) return -0.5 * (Math.Sqrt(1 - k * k) - 1);
            return 0.5 * (Math.Sqrt(1 - (k -= 2) * k) + 1);
        }
    }

    public class ElasticInFunction : Easing
    {
        public override double Ease(double k)
        {
            if (k == 0) return 0;
            if (k == 1) return 1;
            return -Math.Pow(2, 10 * (k -= 1)) * Math.Sin((k - 0.1) * (2 * Math.PI) / 0.4);
        }
    }

    public class ElasticOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if (k == 0) return 0;
            if (k == 1) return 1;
            return Math.Pow(2, -10 * k) * Math.Sin((k - 0.1) * (2 * Math.PI) / 0.4) + 1;
        }
    }

    public class ElasticInOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if ((k *= 2) < 1) return -0.5 * Math.Pow(2, 10 * (k -= 1)) * Math.Sin((k - 0.1) * (2 * Math.PI) / 0.4);
            return Math.Pow(2, -10 * (k -= 1)) * Math.Sin((k - 0.1) * (2 * Math.PI) / 0.4) * 0.5 + 1;
        }
    }

    public class BackInFunction : Easing
    {

        public override double Ease(double k)
        {
            return k * k * ((s + 1) * k - s);
        }
    }

    public class BackOutFunction : Easing
    {
        public override double Ease(double k)
        {
            return (k -= 1) * k * ((s + 1) * k + s) + 1;
        }
    }

    public class BackInOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if ((k *= 2) < 1) return 0.5 * (k * k * ((s2 + 1) * k - s2));
            return 0.5 * ((k -= 2) * k * ((s2 + 1) * k + s2) + 2);
        }
    }

    public class BounceInFunction : Easing
    {
        public override double Ease(double k)
        {
            return 1 - Easing.BounceOut.Ease(1 - k);
        }
    }

    public class BounceOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if (k < (1 / 2.75))
            {
                return 7.5625 * k * k;
            }
            else if (k < (2 / 2.75))
            {
                return 7.5625 * (k -= (1.5 / 2.75)) * k + 0.75;
            }
            else if (k < (2.5 / 2.75))
            {
                return 7.5625 * (k -= (2.25 / 2.75)) * k + 0.9375;
            }
            else
            {
                return 7.5625 * (k -= (2.625 / 2.75)) * k + 0.984375;
            }
        }
    }

    public class BounceInOutFunction : Easing
    {
        public override double Ease(double k)
        {
            if (k < 0.5) return Easing.BounceIn.Ease(k * 2) * 0.5;
            return Easing.BounceOut.Ease(k * 2 - 1) * 0.5 + 0.5;
        }
    }

}
