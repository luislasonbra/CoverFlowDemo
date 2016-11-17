using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverFlowDemo
{
    public abstract class BaseAnimation
    {
        protected double mDuration;
        protected double mFrom;
        protected double mTo;

        public BaseAnimation(double from, double to, double duration)
        {
            mFrom = from;
            mTo = to;
            mDuration = duration;
        }

        public void SetFrom(double from)
        {
            mFrom = from;
        }

        public void SetTo(double end)
        {
            mTo = end;
        }

        public  double GetValue(double time)
        {
            time = Math.Min(Math.Max(time, 0), mDuration);
            return mFrom + (mTo - mFrom) * ComputeValue(time / mDuration);
        }

        public abstract double ComputeValue(double normalizedTime);
 
    }

    public class LinearAnimation : BaseAnimation
    {
        public LinearAnimation(double from, double to, double duration) : base(from, to, duration)
        {
        }

        public override double ComputeValue(double normalizedTime)
        {
            return  normalizedTime;
        }
    }

    public enum EasingMode
    {
        EaseIn,    // the easing is performed at the start of the animation
        EaseOut,   // the easing is performed at the end of the animation
        EaseInOut, // the easing is performed both at the start and the end of the animation
    }

    public abstract class EaseBaseAnimation : BaseAnimation
    {
        public EasingMode EasingMode;

        public EaseBaseAnimation(double from, double to, double duration) : base(from, to, duration)
        {
        }

        public double Ease(double normalizedTime)
        {
            switch (EasingMode)
            {
                case EasingMode.EaseIn:
                    return ComputeValue(normalizedTime);
                case EasingMode.EaseOut:
                    // EaseOut is the same as EaseIn, except time is reversed & the result is flipped.
                    return 1.0 - ComputeValue(1.0 - normalizedTime);
                case EasingMode.EaseInOut:
                default:
                    // EaseInOut is a combination of EaseIn & EaseOut fit to the 0-1, 0-1 range.
                    return (normalizedTime < 0.5) ?
                               ComputeValue(normalizedTime * 2.0) * 0.5 :
                        (1.0 - ComputeValue((1.0 - normalizedTime) * 2.0)) * 0.5 + 0.5;
            }
        }

        public new double GetValue(double time)
        {
            time = Math.Min(Math.Max(time, 0), mDuration);
            return mFrom + (mTo - mFrom) * Ease(time / mDuration);
        }
    }

    public class ElasticEaseAnimation : EaseBaseAnimation
    {
        private int mOscillations;
        private double mSpringiness;

        public ElasticEaseAnimation(int oscillations, double springiness, double from, double to, double duration)
            :base(from, to, duration)
        {
            mOscillations = oscillations;
            mSpringiness = springiness;
        }

        public override double ComputeValue(double normalizedTime)
        {
            double oscillations = Math.Max(0.0, (double)mOscillations);
            double springiness = Math.Max(0.0, mSpringiness);
            double expo;
            if (springiness == 0)
            {
                expo = normalizedTime;
            }
            else
            {
                expo = (Math.Exp(springiness * normalizedTime) - 1.0) / (Math.Exp(springiness) - 1.0);
            }

            return expo * (Math.Sin((Math.PI * 2.0 * oscillations + Math.PI * 0.5) * normalizedTime));

        }
    }
}
