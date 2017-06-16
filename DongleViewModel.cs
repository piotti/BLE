using CySmart.DongleCommunicator.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE
{
    class DongleViewModel : INotifyPropertyChanged
    {

        public DongleViewModel()
        {
            Temperature = "";
            Pressure = "";
            StackAddress = "";
            dongle = new Dongle(this);

        }

        public bool recording;
        public List<double> temps = new List<double>();
        public List<double> setpoints = new List<double>();
        public List<double> times = new List<double>();
        public long startTime;
        private int setpoint = 0;

        private Dongle dongle;
        private string _temperature;
        public string Temperature
        {
            get { return _temperature; }
            set
            {
                if (recording)
                {
                    temps.Add(Double.Parse(value));
                    times.Add(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime);
                    setpoints.Add(setpoint);
                }
                _temperature = value + " deg C";
                NotifyPropertyChanged("Temperature");
            }
        }

        private string _pressure;
        public string Pressure
        {
            get { return _pressure; }
            set
            {
                _pressure = value + " psi";
                NotifyPropertyChanged("Pressure");
            }
        }
        private string _StackAddress;
        public string StackAddress
        {
            get { return _StackAddress; }
            set
            {
                _StackAddress = value;
                NotifyPropertyChanged("StackAddress");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public CyApiErr turnOnTempNotifs()
        {
            return dongle.turnOnTempNotifs();
        }
        public CyApiErr turnOffTempNotifs()
        {
            return dongle.turnOffTempNotifs();
        }
        public CyApiErr turnOnPressureNotifs()
        {
            return dongle.turnOnPressureNotifs();
        }
        public CyApiErr turnOffPressureNotifs()
        {
            return dongle.turnOffPressureNotifs();
        }
        public CyApiErr connect(string com)
        {
            return dongle.connect(com);
        }
        public void disconnect()
        {
            dongle.disconnect();
        }
        public CyApiErr readAddress()
        {
            return dongle.readAddress();
        }
        public CyApiErr updateSetpoint(int setpoint)
        {
            
            CyApiErr err =  dongle.updateSetpoint(setpoint);
            if (err.IsOk)
                this.setpoint = setpoint;
            return err;
        }
        public CyApiErr turnOnThermoController()
        {
            return dongle.turnOnThermoController();
        }
        public CyApiErr turnOffThermoController()
        {
            return dongle.turnOffThermoController();
        }
        public CyApiErr updateMotorSpeed(int speed)
        {
            return dongle.updateMotorSpeed(speed);
        }
        public CyApiErr hapticPreset(int preset)
        {
            return dongle.hapticPreset(preset);
        }
    }
}

