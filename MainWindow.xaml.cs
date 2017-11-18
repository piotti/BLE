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
using System.Windows.Controls.Primitives;
using Npgsql;
using System.Diagnostics;

namespace BLE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool connected = false;

        private bool readSuccess = false;
        private bool startRead = false;

        private DongleViewModel dvm;
        


        public MainWindow()
        {
            
            dvm = new DongleViewModel();
            this.DataContext = dvm;
            

            InitializeComponent();

            System.Threading.Thread newThread;
            newThread = new System.Threading.Thread(this.runThread);
            newThread.Start();
        }

        private void runThread()
        {
            Stopwatch watch = Stopwatch.StartNew();
            Stopwatch read_time = null;
            System.Threading.Thread newThread;
            newThread = new System.Threading.Thread(this.readAsyncAddress);
            newThread.Start();
            bool reading = false;
            while (true)
            {
                if (watch.ElapsedMilliseconds >= 5000)
                {
                    Console.WriteLine("tick");

                    if (connected)
                    {
                        if (!reading)
                        {
                            //Read address
                            readSuccess = false;
                            reading = true;
                            startRead = true;

                            read_time = Stopwatch.StartNew();
                        }
                        else
                        {
                            if (read_time.ElapsedMilliseconds >= 5000)
                            {
                                //check if read was successful
                                if (readSuccess)
                                {
                                    Console.WriteLine("read successful");
                                }
                                else
                                {
                                    //took too long
                                    Console.WriteLine("timeout");
                                    dvm.disconnect();
                                    MessageBox.Show("connection lost");
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        disconnect();
                                    });

                                }
                                reading = false;

                            }
                        }
                    }
                    watch.Reset();
                    watch.Start();


                }
            }
        }

        private void readAsyncAddress()
        {
            while (true)
            {
                if (startRead)
                {
                    Console.WriteLine("reading...");
                    if (dvm.readAddress().IsOk)
                    {
                        readSuccess = true;
                    }
                    startRead = false;
                }
            }

        }


        // Called whenever an API call returns an error. Displays error then disconnects
        private bool processError(CyApiErr err)
        {
            if (err.IsNotOk)
            {
                MessageBox.Show(err.Message);
                dvm.disconnect();
            }
            return err.IsNotOk;
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
            
            CyApiErr err = dvm.turnOnTempNotifs();
            if (processError(err))
            {
                tempNotifCheck.IsChecked = false;
            }
        }

        private void tempNotifCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOffTempNotifs();
            if (processError(err))
            {
                tempNotifCheck.IsChecked = true;
            }
        }

        private void pressureNotifCheck_Checked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOnPressureNotifs();
            if (processError(err))
            {
                pressureNotifCheck.IsChecked = false;
            }
        }

        private void pressureNotifCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOffPressureNotifs();
            if (processError(err))
            {
                pressureNotifCheck.IsChecked = true;
            }
        }

        private void connect()
        {
            connectBtn.Content = "Disconnect";
            connected = true;
            tempNotifCheck.IsEnabled = true;
            pressureNotifCheck.IsEnabled = true;
            readAddrBtn.IsEnabled = true;
            thermoControllerCheck.IsEnabled = true;
            hapticPresetSendBox.IsEnabled = true;
            motorSlider.IsEnabled = true;
            setpointSlider.IsEnabled = true;
            record.IsEnabled = true;
            commandInput.IsEnabled = true;
            commandSend.IsEnabled = true;

            portNameBox.IsEnabled = false;
            psocComboBox.IsEnabled = false;
        }

        private void disconnect()
        {
            connectBtn.Content = "Connect";
            connected = false;
            tempNotifCheck.IsEnabled = false;
            pressureNotifCheck.IsEnabled = false;
            readAddrBtn.IsEnabled = false;
            thermoControllerCheck.IsEnabled = false;
            hapticPresetSendBox.IsEnabled = false;
            motorSlider.IsEnabled = false;
            setpointSlider.IsEnabled = false;
            record.IsEnabled = false;
            commandInput.IsEnabled = false;
            commandSend.IsEnabled = false;

            portNameBox.IsEnabled = true;
            psocComboBox.IsEnabled = true;
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!connected)
            {
                string com = portNameBox.Text;
                string psocName = ((ComboBoxItem)psocComboBox.SelectedItem).Content.ToString();
                CyApiErr err = dvm.connect(com, psocName);
                if (!processError(err))
                {
                    connect();
                }
            } else
            {
                dvm.disconnect();
                disconnect();
                //Application.Current.Shutdown();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.readAddress();
            processError(err);
        }

        private bool dragStarted = false;

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            int temp = (int)((20 + ((Slider)sender).Value * 28) / 5) * 5;
            CyApiErr err = dvm.updateSetpoint(temp);
            processError(err);
            this.dragStarted = false;
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.dragStarted = true;
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int temp = (int)((20 + e.NewValue * 28) / 5) * 5;
            setpointBox.Text = "Setpoint: " + temp + " ° C";
            if (!dragStarted)
            {
                CyApiErr err = dvm.updateSetpoint(temp);
                processError(err);
            }


        }

        private bool motorDragStarted = false;

        private void motorSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            int perc = (int)(((Slider)sender).Value*2) * 5;
            CyApiErr err = dvm.updateMotorSpeed(perc);
            processError(err);
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
                processError(err);
            }

            
        }

        private void thermoControllerCheck_Checked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOnThermoController();
            processError(err);
        }
        private void thermoControllerCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CyApiErr err = dvm.turnOffThermoController();
            processError(err);
        }

        private void hapticPresetSendBox_Click(object sender, RoutedEventArgs e)
        {
            int preset = Int32.Parse(hapticPresetBox.Text);
            CyApiErr err = dvm.hapticPreset(preset);
            processError(err);

        }

        
        private void record_Checked(object sender, RoutedEventArgs e)
        {
            dvm.startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            dvm.temps.Clear();
            dvm.setpoints.Clear();
            dvm.times.Clear();
            dvm.outs.Clear();
            dvm.addDataPoint();
            dvm.recording = true;
        }
        
            

        private void record_Unchecked(object sender, RoutedEventArgs e)
        {
            dvm.recording = false;
            dvm.addDataPoint();
            System.IO.StreamWriter file = new System.IO.StreamWriter("data.txt");
            for(int i = 0; i < dvm.times.Count; i++)
            {
                file.WriteLine(String.Format("{0},{1},{2},{3}", dvm.times[i], dvm.temps[i], dvm.setpoints[i], dvm.outs[i]));
            }

            file.Close();
            System.Diagnostics.Process.Start(@"graph.py");

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connected)
                dvm.disconnect();
        }

        private void inputKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                dvm.sendCommand(commandInput.Text);
                commandInput.Text = "";
            }
        }

        private void commandSend_Click(object sender, RoutedEventArgs e)
        {
            dvm.sendCommand(commandInput.Text);
            commandInput.Text = "";
        }

        private void portNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
