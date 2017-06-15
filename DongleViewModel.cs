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



        private Dongle dongle;
        private string _temperature;
        public string Temperature
        {
            get { return _temperature; }
            set
            {
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
            return dongle.updateSetpoint(setpoint);
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

