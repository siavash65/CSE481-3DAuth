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

namespace ThreeDAuth
{
    /// <summary>
    /// Interaction logic for HandTrackingOptions.xaml
    /// </summary>
    public partial class HandTrackingOptionsWindow : Window
    {
        private Window parent;

        private bool initialized;

        public HandTrackingOptionsWindow(Window parent)
        {
            this.parent = parent;
            initialized = false;

            InitializeComponent();

            ScaleToFillScreenCB.IsChecked = HandTrackingOptionSet.ProjectingPoints;
            ShowUnprojectedHandCB.IsChecked = HandTrackingOptionSet.ShowUnprojectedHand;
            AllowTorsoMovementCB.IsChecked = HandTrackingOptionSet.AllowingTorsoMotion;
            ShowTargetPointsCB.IsChecked = HandTrackingOptionSet.ShowTargetPoints;
            CornerAlphaSlider.Value = HandTrackingOptionSet.Alpha;
            CornerAlphaTextBox.Text = "" + HandTrackingOptionSet.Alpha;
            FloodFillDepthTextBox.Text = HandTrackingOptionSet.FloodFillDepth + " mm";

            initialized = true;
        }
        
        private void ScaleToFillScreenClicked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.IsChecked != null)
            {
                HandTrackingOptionSet.ProjectingPoints = (bool) checkbox.IsChecked;
            }
        }

        private void AllowTorsoMovementClicked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.IsChecked != null)
            {
                HandTrackingOptionSet.AllowingTorsoMotion = (bool)checkbox.IsChecked;
            }
        }

        private void UseDepthClicked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.IsChecked != null)
            {
                HandTrackingOptionSet.ShowTargetPoints = (bool)checkbox.IsChecked;
            }
        }

        private void ShowUnprojectedHandClicked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.IsChecked != null)
            {
                HandTrackingOptionSet.ShowUnprojectedHand = (bool)checkbox.IsChecked;
            }
        }

        private void CornerAlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            HandTrackingOptionSet.Alpha = slider.Value;
            if (initialized)
            {
                CornerAlphaTextBox.Text = "" + HandTrackingOptionSet.Alpha;
            }
        }

        private void FloodFilleDepthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            HandTrackingOptionSet.FloodFillDepth = (int) slider.Value;
            if (initialized)
            {
                FloodFillDepthTextBox.Text = HandTrackingOptionSet.FloodFillDepth + " mm";
            }
        }

        private void ShowTargetPoints(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.IsChecked != null)
            {
                HandTrackingOptionSet.ShowTargetPoints = (bool)checkbox.IsChecked;
            }
        }
    }
}
