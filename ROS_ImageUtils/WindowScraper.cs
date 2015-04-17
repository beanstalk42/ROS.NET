﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Messages.sensor_msgs;
using Microsoft.Win32.SafeHandles;
using Ros_CSharp;
using Image = Messages.sensor_msgs.Image;

namespace ROS_ImageWPF
{
    public class WindowScraper
    {
        private Timer timer;
        private Publisher<CompressedImage> pub;
        private Window window;
        private int period_ms = Timeout.Infinite;
        private bool running = false;
        private double dpiX, dpiY;
        private double WIDTH, HEIGHT;
        private bool enabled = false;
        private NodeHandle nh;

        public WindowScraper(string topic, Window w, int hz = 1) : this(topic, w, TimeSpan.FromSeconds(1.0/((double) hz)))
        {
        }

        public WindowScraper(string topic, Window w, TimeSpan period)
        {
            window = w;
            window.Dispatcher.Invoke(new Action(() =>
            {
                w.SizeChanged += new SizeChangedEventHandler(w_SizeChanged);
                WIDTH = w.RenderSize.Width;
                HEIGHT = w.RenderSize.Height;
            }));
            period_ms = (int) Math.Floor(period.TotalMilliseconds);
            timer = new Timer(callback, null, Timeout.Infinite, period_ms);
            nh = new NodeHandle();
            if (!topic.EndsWith("/compressed"))
            {
                Console.WriteLine("APPENDING /compressed TO TOPIC NAME TO MAKE IMAGE TRANSPORT HAPPY");
                topic += "/compressed";
            }
            pub = nh.advertise<CompressedImage>(topic, 10);
        }

        private void w_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WIDTH = e.NewSize.Width;
            HEIGHT = e.NewSize.Height;
        }

        public void Start()
        {
            if (!enabled)
            {
                enabled = true;
                timer.Change(0, period_ms);
            }
        }

        public void Stop()
        {
            if (enabled)
            {
                enabled = false;
                timer.Change(Timeout.Infinite, period_ms);
            }
        }

        public void Shutdown()
        {
            Stop();
            pub.shutdown();
            ROS.shutdown();
            ROS.waitForShutdown();
        }

        private void callback(object o)
        {
            if (dpiX <= 0 || dpiY <= 0)
            {
                window.Dispatcher.Invoke(new Action(() =>
                {
                    PresentationSource source = PresentationSource.FromVisual(window);

                    if (source != null)
                    {
                        dpiX = 96.0*source.CompositionTarget.TransformToDevice.M11;
                        dpiY = 96.0*source.CompositionTarget.TransformToDevice.M22;
                    }
                }));
            }

            window.Dispatcher.Invoke(new Action(() =>
            {
                CompressedImage cm = new CompressedImage { format = new Messages.std_msgs.String("jpeg"), header = new Messages.std_msgs.Header { stamp = ROS.GetTime() } };
                #region based on http://blogs.msdn.com/b/saveenr/archive/2008/09/18/wpf-xaml-saving-a-window-or-canvas-as-a-png-bitmap.aspx

                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    (int) WIDTH, //width 
                    (int) HEIGHT, //height 
                    dpiX, //dpi x 
                    dpiY, //dpi y 
                    PixelFormats.Pbgra32 // pixelformat 
                    );
                rtb.Render(window);

                JpegBitmapEncoder enc = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));

                using (MemoryStream ms = new MemoryStream())
                {
                    enc.Save(ms);
                    ms.Flush();
                    cm.data = ms.GetBuffer();
                    ms.Close();
                }

                pub.publish(cm);

                #endregion
            }));
        }
    }
}
