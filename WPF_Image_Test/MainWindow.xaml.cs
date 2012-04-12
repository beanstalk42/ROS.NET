﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Messages;
using Messages.custom_msgs;
using Ros_CSharp;
using XmlRpc_Wrapper;
using Int32 = Messages.std_msgs.Int32;
using String = Messages.std_msgs.String;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;

namespace WPF_Image_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ROS.ROS_MASTER_URI = "http://robot-lab9:11311";
            IPAddress[] addr = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress v4 = null;
            foreach (IPAddress a in addr)
            {
                if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && a != IPAddress.Loopback && (!a.ToString().Contains("192.168.2")))
                    v4 = a;
            }
            ROS.ROS_HOSTNAME = v4.ToString();
            ROS.Init(new string[0], "ImageTest");
        }
    }
}
