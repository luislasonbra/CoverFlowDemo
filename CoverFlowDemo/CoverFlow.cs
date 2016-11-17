using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace CoverFlowDemo
{
    public class CoverFlow
    {
        const int ITEM_MARGIN = 100;
        const float SCALE_FACTOR = 0.8f;
        const double ANIMATION_DURATION = 0.3;
        const int SHOW_ITEMS = 9;
        private CoverFlowItem[] mItems;
        List<CanvasBitmap> mProducts;
        private int mWidth;
        private int mHeight;
        private int mItemWidth;
        private int mItemHeight;
        private int mSelectIndex = 2;
        private Point mCenter;
        private double mTimer;
        private AnimatingStatus mAnimatingStatus;

        public int VisualChildCount
        {
            get { return mItems.Length; }
        }

        private int Count
        {
            get { return mProducts.Count; }
        }

        public CoverFlow(int width, int height, int itemWidth, int itemHeight)
        {
            mWidth = width;
            mHeight = height;
            mItemWidth = itemWidth;
            mItemHeight = itemHeight;
            mCenter = new Point(mWidth / 2, mHeight / 2);
        }

        public void LoadProducts(List<CanvasBitmap> products)
        {
            mProducts = products;
        }

        public void Initialize()
        {
            mItems = new CoverFlowItem[SHOW_ITEMS + 2];
            for (int i = 0; i < mItems.Length; i++)
            {
                mItems[i] = new CoverFlowItem(ANIMATION_DURATION);
            }
            RefreshItems();
        }

        private void RefreshItems()
        {
            int centerIndex = VisualChildCount / 2;
            mItems[centerIndex].CurrentPosition = mCenter;
            mItems[centerIndex].CurrentScale = 1;
            mItems[centerIndex].CurrentOpacity = 1;
            mItems[centerIndex].Index = mSelectIndex;
            for (int i = 1; i <= centerIndex; i++)
            {
                var left = centerIndex - i;
                var right = centerIndex + i;
                mItems[left].CurrentPosition = new Point(mCenter.X - i * ITEM_MARGIN, mCenter.Y);
                mItems[right].CurrentPosition = new Point(mCenter.X + i * ITEM_MARGIN, mCenter.Y);
                mItems[left].CurrentScale = Math.Pow(SCALE_FACTOR, i);
                mItems[right].CurrentScale = Math.Pow(SCALE_FACTOR, i);
                mItems[left].CurrentOpacity = i == centerIndex ? 0 : 1;
                mItems[right].CurrentOpacity = i == centerIndex ? 0 : 1;
                mItems[left].Index = (mSelectIndex - i + Count) % Count;
                mItems[right].Index = (mSelectIndex + i + Count) % Count;
            }
        }

        public void Update(double delta)
        {
            if (mAnimatingStatus == AnimatingStatus.None)
                return;

            for (int i = 0; i < mItems.Length; i++)
            {
                mItems[i].Update(mTimer);
            }
            mTimer += delta;
            if (mTimer >= ANIMATION_DURATION)
            {
                mAnimatingStatus = AnimatingStatus.None;
                mTimer = 0;
                RefreshItems();
            }
        }

        public void FlipRight()
        {
            if (mAnimatingStatus > 0)
                return;

            mSelectIndex = (mSelectIndex - 1 + Count) % Count;
            mAnimatingStatus = AnimatingStatus.Right;
            for (int i = 0; i < VisualChildCount - 1; i++)
            {
                var nextItem = mItems[i + 1];
                mItems[i].Flip(nextItem.CurrentPosition, nextItem.CurrentScale, nextItem.CurrentOpacity);
            }
        }

        public void FlipLeft()
        {
            if (mAnimatingStatus > 0)
                return;

            mSelectIndex = (mSelectIndex + 1 + Count) % Count;
            mAnimatingStatus = AnimatingStatus.Left;
            for (int i = VisualChildCount - 1; i > 0; i--)
            {
                var lastItem = mItems[i - 1];
                mItems[i].Flip(lastItem.CurrentPosition, lastItem.CurrentScale, lastItem.CurrentOpacity);
            }
        }

        public void Draw(CanvasDrawingSession graphics)
        {
            var activeIndex = VisualChildCount / 2;
            if (mAnimatingStatus == AnimatingStatus.Left)
            {
                activeIndex = activeIndex + 1;
            }
            if (mAnimatingStatus == AnimatingStatus.Right)
            {
                activeIndex = activeIndex - 1;
            }
            for (int i = VisualChildCount / 2; i >= 1; i--)
            {
                //draw left
                if (activeIndex - i >= 0)
                {
                    DrawCoverItem(graphics, mItems[activeIndex - i]);
                }
                //draw right;
                if (activeIndex + i < VisualChildCount)
                {
                    DrawCoverItem(graphics, mItems[activeIndex + i]);
                }
            }
            //draw center
            DrawCoverItem(graphics, mItems[activeIndex]);
        }

        private void DrawCoverItem(CanvasDrawingSession graphics, CoverFlowItem item)
        {
            var center = item.CurrentPosition;
            var width = mItemWidth * item.CurrentScale;
            var height = mItemHeight * item.CurrentScale;
            using (graphics.CreateLayer((float)item.CurrentOpacity))
            {
                graphics.DrawImage(mProducts[item.Index], new Rect(center.X - width / 2, center.Y - height / 2, width, height));
            }
        }
    }

    public enum AnimatingStatus
    {
        None,
        Left,
        Right
    }

    public class CoverFlowItem
    {
        public int Index;
        public Point CurrentPosition;
        public double CurrentScale;
        public double CurrentOpacity;

        private LinearAnimation mTranslateAnimation;
        private LinearAnimation mScaleAnimation;
        private LinearAnimation mOpacityAnimation;

        public CoverFlowItem(double duration)
        {
            mTranslateAnimation = new LinearAnimation(0, 0, duration);
            mScaleAnimation = new LinearAnimation(0, 0, duration);
            mOpacityAnimation = new LinearAnimation(0, 0, duration);
        }

        public void Flip(Point position, double scale, double opacity)
        {
            mTranslateAnimation.SetFrom(CurrentPosition.X);
            mTranslateAnimation.SetTo(position.X);

            mScaleAnimation.SetFrom(CurrentScale);
            mScaleAnimation.SetTo(scale);

            mOpacityAnimation.SetFrom(CurrentOpacity);
            mOpacityAnimation.SetTo(opacity);
        }

        public void Update(double time)
        {
            CurrentPosition.X = (float)mTranslateAnimation.GetValue(time);
            CurrentScale = mScaleAnimation.GetValue(time);
            CurrentOpacity = mOpacityAnimation.GetValue(time);
        }
    }
}
