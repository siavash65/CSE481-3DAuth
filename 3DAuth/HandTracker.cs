using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ThreeDAuth
{
    class HandTracker
    {
        private KinectSensor sensor;
        private KinectSensor kinectSensor;
        private Point3d hand;
        private short[] pixelData;
        private DepthImagePixel[] imagePixelData;
        private List<DepthImagePixel> imagePixelList;
        private int maxDepth;
        private int minDepth;
        private DepthImagePixel closestPoint;
        private int counter = 0;
        public Bitmap bmap;


        public HandTracker(KinectSensor mySensor)
        {
            this.kinectSensor = mySensor;
            this.kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            this.kinectSensor.DepthFrameReady += this.sensorDepthFrameReady;
            this.hand = new Point3d();
            this.imagePixelList = new List<DepthImagePixel>();
            this.closestPoint = new DepthImagePixel();
            
            
            // Start the sensor!
            try
            {
                this.kinectSensor.Start();
            }
            catch (IOException)
            {
                this.kinectSensor = null;
            }
            
        }



        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void sensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    this.maxDepth = depthFrame.MaxDepth;
                    this.minDepth = depthFrame.MinDepth;
                    this.closestPoint.Depth = Convert.ToInt16(this.maxDepth);
                    
                    if (this.imagePixelData == null)
                    {
                        this.pixelData = new short[depthFrame.PixelDataLength];
                        this.imagePixelData = new DepthImagePixel[depthFrame.PixelDataLength];
                    }

                    depthFrame.CopyDepthImagePixelDataTo(imagePixelData);
                    depthFrame.CopyPixelDataTo(pixelData);
                    showDepthView(depthFrame.Width,depthFrame.Height);
                    findTheClosestPoint(depthFrame.PixelDataLength);
                    
                }
            }  
        }

        private void showDepthView(int width, int height)
        {
            bmap = new Bitmap(width,height,PixelFormat.Format16bppRgb555);
            BitmapData bmapdata = bmap.LockBits(new Rectangle(0, 0, width,height),ImageLockMode.WriteOnly,bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(pixelData,0,ptr,width *height);
            bmap.UnlockBits(bmapdata);
             

            
        }

        private void findTheClosestPoint(int pixelDataLenght)
        {
            for (int i = 0; i < pixelDataLenght; i++)
            {
                if (this.imagePixelData[i].IsKnownDepth == true)
                {
                    this.imagePixelList.Add(this.imagePixelData[i]);
                }
            }

            foreach (DepthImagePixel pixle in imagePixelList)
            {
                if (pixle.Depth > minDepth && pixle.Depth < this.closestPoint.Depth)
                {
                    this.closestPoint = pixle;
                    
                }
            }

            Console.WriteLine("The closest point depth is: " + this.closestPoint.Depth + " ( " + this.counter + " )");
            this.counter++;
            this.imagePixelList.Clear();
        }
     
    }
}
