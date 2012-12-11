using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Samples.Kinect.SkeletonBasics;
using Microsoft.Kinect;
using System.Drawing;
using System.Drawing.Imaging;

namespace ThreeDAuth
{
    /// <summary>
    /// Interaction logic for DepthViewWindow.xaml
    /// </summary>
    partial class DepthViewWindow : Window
    {
        private bool _running;
        public bool Running
        {
            get
            {
                return _running;
            }
        }

        private DrawingGroup drawingGroup;

        public DepthViewWindow(MainWindow parent)
        {
            InitializeComponent();
            _running = true;
            drawingGroup = new DrawingGroup();
            //parent.OnDepthFrameReceived += new GiveDepthFrame(GiveFrame);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _running = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs args)
        {
            this.myImageBox.Source = new DrawingImage(drawingGroup);
        }

        internal void GiveFrame(DepthImageFrame depthFrame, PointCluster floodFill)
        {
            short[] imagePixelData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(imagePixelData);
            
            float RenderWidth = 640.0f;
            float RenderHeight = 480.0f;

            Bitmap bmap = new System.Drawing.Bitmap(depthFrame.Width, depthFrame.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            System.Drawing.Imaging.BitmapData bmapdata = bmap.LockBits(new System.Drawing.Rectangle(0, 0, depthFrame.Width
                , depthFrame.Height), ImageLockMode.WriteOnly, bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(imagePixelData, 0, ptr, depthFrame.Width * depthFrame.Height);
            bmap.UnlockBits(bmapdata);
            /*System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmap);
            this.myImageBox.Source =
            System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight((int)this.myImageBox.Width, (int)this.myImageBox.Height));*/

            using (DrawingContext lfdc = drawingGroup.Open())//this.liveFeedbackGroup.Open())
            {
                lfdc.DrawImage(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bmap.GetHbitmap(),
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight((int)RenderWidth, (int)RenderHeight)),
                    //BitmapSizeOptions.FromWidthAndHeight((int)this.myImageBox.ActualWidth, (int)this.myImageBox.ActualHeight)),
                    new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                foreach (DepthPoint point in floodFill.points)
                {
                    lfdc.DrawRoundedRectangle(System.Windows.Media.Brushes.Red, null, new Rect(point.x, point.y, 3, 3), null, 1, null, 1, null);
                }
            }
        }
    }
}
