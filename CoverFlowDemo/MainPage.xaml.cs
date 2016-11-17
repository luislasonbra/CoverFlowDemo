using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CoverFlowDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<CanvasBitmap> mBitmaps = new List<CanvasBitmap>();
        const double FLIP_THRESHHOLD = 10;
        Vector2 mCenter;
        CoverFlow coverFlow;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(LoadResources(sender).AsAsyncAction());
        }

        async Task LoadResources(ICanvasResourceCreator creator)
        {
            mBitmaps.Clear();
            StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Products");
            var files = await assets.GetFilesAsync();
            foreach (var file in files)
            {
                var bitmap = await CanvasBitmap.LoadAsync(creator, file.Path);
                mBitmaps.Add(bitmap);
            }
            coverFlow = new CoverFlow((int)canvas.ActualWidth, (int)canvas.ActualHeight, 300, 300);
            coverFlow.LoadProducts(mBitmaps);
            coverFlow.Initialize();
        }

        private void canvas_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
            coverFlow.Update(args.Timing.ElapsedTime.TotalSeconds);
        }

        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            //draw current item
            coverFlow.Draw(args.DrawingSession);
        }


        Point mLastPosition = new Point(double.NaN, double.NaN);
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            mLastPosition = e.GetCurrentPoint(canvas).Position;
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);

        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (!double.IsNaN(mLastPosition.X))
            {
                var currentPosition = e.GetCurrentPoint(canvas).Position;
                if (Math.Abs(currentPosition.X - mLastPosition.X) > FLIP_THRESHHOLD)
                {
                    if (currentPosition.X > mLastPosition.X)
                    {
                        coverFlow.FlipRight();
                    }
                    else
                    {
                        coverFlow.FlipLeft();
                    }
                }
            }
            mLastPosition = new Point(double.NaN, double.NaN);
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mCenter = new Vector2((float)e.NewSize.Width / 2, (float)e.NewSize.Height / 2);

        }


    }


}
