using System;
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

namespace BLE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool connected = false;
        private Dongle dongle;



        public MainWindow()
        {
            InitializeComponent();
            dongle = new Dongle();
            this.DataContext = dongle;
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
            
            CyApiErr err = dongle.turnOnTempNotifs();
            if (err.IsNotOk)
            {
                MessageBox.Show(err.Message);
                tempNotifCheck.IsChecked = false;
            }
        }

        private void tempNotifCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dongle.turnOffTempNotifs();
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
            CyApiErr err = dongle.turnOnPressureNotifs();
            if (err.IsNotOk)
            {
                MessageBox.Show(err.Message);
                pressureNotifCheck.IsChecked = false;
            }
        }

        private void pressureNotifCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dongle.turnOffPressureNotifs();
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
                
                CyApiErr err = dongle.connect(com);
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
                //disconnect
                connectBtn.Content = "Connect";
                connected = false;
            }
        }
    }
}
