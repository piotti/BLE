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

namespace BLE
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
            Dongle.turnOnTempNotifs();
        }

        private void tempNotifCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            Dongle.turnOffTempNotifs();
        }

        private void pressureNotifCheck_Checked(object sender, RoutedEventArgs e)
        {
            Dongle.turnOnPressureNotifs();
        }

        private void pressureNotifCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            Dongle.turnOffPressureNotifs();
        }
    }
}
