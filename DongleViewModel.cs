using CySmart.DongleCommunicator.API;
using Npgsql;
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

        private NpgsqlConnection conn;

        private Dictionary<string, string> database_psoc_names;
        private string db_handle;

        public DongleViewModel()
        {

            database_psoc_names = new Dictionary<string, string>();
            database_psoc_names.Add("00795", "1");
            database_psoc_names.Add("00392", "2");
           

            conn = new NpgsqlConnection("Host = localhost; Username = postgres; Password = JensenRobot321; Database = robot_info");
            conn.Open();

            
            Temperature = "";
            Pressure = "";
            StackAddress = "";
            dongle = new Dongle(this);

        }

        public bool recording;
        public List<double> temps = new List<double>();
        public List<double> setpoints = new List<double>();
        public List<double> times = new List<double>();
        public List<int> outs = new List<int>();
        public long startTime;
        private int setpoint = 0;

        private bool controller_on = false;



        private Dongle dongle;
        private double lastTemp = 0;
        public void addDataPoint()
        {
            temps.Add(lastTemp);
            times.Add(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime);
            setpoints.Add(controller_on ? setpoint : 0);
            outs.Add(Output);
        }
        

        private void updateDatabase(string field, string val)
        {
            try
            {
                var cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = String.Format("UPDATE real_time SET {0}={1} WHERE info_id=1", field, val);
                cmd.ExecuteNonQuery();
            }
            catch (Npgsql.PostgresException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private string _temperature;
        public string Temperature
        {
            get { return _temperature; }
            set
            {
                if(lastTemp != 0)
                    addDataPoint();
                if(value != "")
                    lastTemp = Double.Parse(value) / 100;
                if (recording)
                {
                    addDataPoint();
                }
                if(value != "")
                {
                    _temperature = String.Format("{0:0.00}°C", lastTemp);
                }
                else
                {
                    _temperature = "°C";
                }
                updateDatabase("temp_" + db_handle, String.Format("{0:00}", lastTemp));
                NotifyPropertyChanged("Temperature");
            }
        }

        public int Output=0;

        private string _pressure;
        private int lastPressure = 0;
        public string Pressure
        {
            get { return _pressure; }
            set
            {
                if (value != "")
                    lastPressure = Int16.Parse(value);
                _pressure = value + " psi";
                updateDatabase("pres_" + db_handle, value);
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
        public CyApiErr connect(string com, string psocName)
        {
            database_psoc_names.TryGetValue(psocName, out db_handle);
            return dongle.connect(com, psocName);
        }
        public void disconnect()
        {
            conn.Close();
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
            controller_on = true;
            return dongle.turnOnThermoController();
        }
        public CyApiErr turnOffThermoController()
        {
            controller_on = false;
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
        public CyApiErr sendCommand(string command)
        {
            return dongle.sendCommand(command);
        }
    }
}

