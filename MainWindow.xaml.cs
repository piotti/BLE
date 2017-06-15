﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CySmart.DongleCommunicator.API;
using System.Windows.Controls.Primitives;

namespace BLE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool connected = false;
        private DongleViewModel dvm;


        public MainWindow()
        {
            InitializeComponent();
            dvm = new DongleViewModel();
            this.DataContext = dvm;


        }

        public void updateTemp(float temp)
        {
            tempBox.Text = "" + temp;
        }
        public void updatePressure(float pressure)
        {
            pressureBox.Text = "" + pressure;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tempNotifCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (!connected) {
                tempNotifCheck.IsChecked = false;
                return;
            }
            
            CyApiErr err = dvm.turnOnTempNotifs();
            if (err.IsNotOk)
            {
                MessageBox.Show(err.Message);
                tempNotifCheck.IsChecked = false;
            }
        }

        private void tempNotifCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOffTempNotifs();
            if (err.IsNotOk)
            {
                MessageBox.Show(err.Message);
                tempNotifCheck.IsChecked = true;
            }
        }

        private void pressureNotifCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (!connected)
            {
                pressureNotifCheck.IsChecked = false;
                return;
            }
            CyApiErr err = dvm.turnOnPressureNotifs();
            if (err.IsNotOk)
            {
                MessageBox.Show(err.Message);
                pressureNotifCheck.IsChecked = false;
            }
        }

        private void pressureNotifCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOffPressureNotifs();
            if (err.IsNotOk)
            {
                MessageBox.Show(err.Message);
                pressureNotifCheck.IsChecked = true;
            }
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!connected)
            {
                string com = portNameBox.Text;
                
                CyApiErr err = dvm.connect(com);
                if (err.IsOk)
                {
                    connectBtn.Content = "Disconnect";
                    connected = true;
                } else
                {
                    MessageBox.Show(err.Message);
                }
            } else
            {
                dvm.disconnect();
                Application.Current.Shutdown();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.readAddress();
            if (err.IsNotOk)
            {
                MessageBox.Show(err.Message);
            }
        }

        private bool dragStarted = false;

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            int temp = (int)((20 + ((Slider)sender).Value * 28) / 5) * 5;
            CyApiErr err = dvm.updateSetpoint(temp);
            if (err.IsNotOk)
                MessageBox.Show(err.Message);
            this.dragStarted = false;
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.dragStarted = true;
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int temp = (int)((20 + e.NewValue * 28) / 5) * 5;
            setpointBox.Text = "Setpoint: " + temp + " deg C";
            if (!dragStarted)
            {
                CyApiErr err = dvm.updateSetpoint(temp);
                if (err.IsNotOk)
                    MessageBox.Show(err.Message);
            }


        }

        private bool motorDragStarted = false;

        private void motorSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            int perc = (int)(((Slider)sender).Value*2) * 5;
            CyApiErr err = dvm.updateMotorSpeed(perc);
            if (err.IsNotOk)
                MessageBox.Show(err.Message);
            this.motorDragStarted = false;
        }

        private void motorSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.motorDragStarted = true;
        }

        private void motorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int perc = (int)(((Slider)sender).Value * 2) * 5;
            motorSpeedBox.Text = perc==0 ? "Off": perc + "%";
            if (!motorDragStarted)
            {
                CyApiErr err = dvm.updateMotorSpeed(perc);
                if (err.IsNotOk)
                    MessageBox.Show(err.Message);
            }

            
        }

        private void thermoControllerCheck_Checked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOnThermoController();
            if (err.IsNotOk)
                MessageBox.Show(err.Message);
        }
        private void thermoControllerCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOffThermoController();
            if (err.IsNotOk)
                MessageBox.Show(err.Message);
        }

        private void hapticPresetSendBox_Click(object sender, RoutedEventArgs e)
        {
            int preset = Int32.Parse(hapticPresetBox.Text);
            CyApiErr err = dvm.hapticPreset(preset);
            if (err.IsNotOk)
                MessageBox.Show(err.Message);

        }
    }
}
