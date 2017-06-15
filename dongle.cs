using System;
using System.Collections.Generic;
using System.Text;

using CySmart.DongleCommunicator.API;
using CySmart.Common.Base.Compatibility.Net20;
using System.Threading;

namespace BLE
{
    #region NotificationLogger

    /// <summary>
    /// Simple class which logs all received notifications to Console
    /// </summary>
    class Dongle : IDisposable
    {


        #region Setup Constants

        // Change the COM port name to the COM port of your CySmart dongle
        const string COM_PORT_NAME = "COM11";

        // Bluetooth address of the peer device you want to connect to
        static readonly CyBleBdAddress PEER_DEVICE_BD_ADDR = 
            new CyBleBdAddress(0x00A050A47A8D, CyBleBdAddressType.PUBLIC_ADDRESS);

        // CCCD attribute handle of the characteristic whose notification needs to be monitored
        const ushort TEMPERATURE_HANDLE = 0x0012; //temp & setpoint
        const ushort TEMPERATURE_CCCD_HANDLE = 0x0013; //notifs
        const ushort PRESSURE_CCCD_HANDLE = 0x001B; //notifs
        const ushort TEMP_CONTROLL_ONOFF_HANDLE = 0x0016; //write
        const ushort HAPTIC_PRESET_HANDLE = 0x001F; //write
        const ushort HAPTIC_BUZZ_HANDLE = 0x0022; //write
        const ushort MOTOR_SPEED_HANDLE = 0x0026; //write
        const ushort STACK_ADDRESS_HANDLE = 0x002A; //read

        #endregion

        #region consts

        // Value to enable notification
        const ushort ENABLE_NOTIFICATION = 0x0001;

        // Value to disable notification
        const ushort DISABLE_NOTIFICATION = 0x0000;

        #endregion

        #region members

        // CySmart dongle communicator instance
        ICySmartDongleCommunicator m_communicator;

        // BLE manager callback
        BleMgrCb m_bleMgrCb;

        // Peer BLE device
        ICyBleDevice m_peerDevice;

        // Device callback
        DeviceCb m_deviceCb;

        // GATT client callback
        GattClientCb m_gattClientCb;

        #endregion

        #region props

        /// <summary>
        /// Gets the BLE manager
        /// </summary>
        ICyBleMgr BleMgr
        {
            get { return m_communicator.BleMgr; }
        }

        /// <summary>
        /// Gets the GATT client
        /// </summary>
        ICyGattClient GattClient
        {
            get { return m_peerDevice.GattClient; }
        }

        #endregion

        #region ctor

        private DongleViewModel dvm;

        public Dongle(DongleViewModel dvm)
        {
            m_communicator = null;
            m_deviceCb = null;
            m_bleMgrCb = null;
            m_peerDevice = null;
            m_gattClientCb = null;

            this.dvm = dvm; 
        }

        #endregion

        #region public methods

        /// <summary>
        /// Connect to CySmart BLE dongle
        /// </summary>
        public CyApiErr ConnectToDongle(CyDongleInfo info)
        {
            CySmartDongleMgr dongleMgr = CySmartDongleMgr.GetInstance();
            CyApiErr err = dongleMgr.TryGetCySmartDongleCommunicator(info, out m_communicator);
            if (err.IsOK)
            {
                m_bleMgrCb = new BleMgrCb();
                err = BleMgr.RegisterBleMgrCallback(m_bleMgrCb);
            }

            return err;
        }


        /// <summary>
        /// Connect to peer BLE device
        /// </summary>
        public CyApiErr ConnectToBleDevice(CyBleBdAddress deviceAddr)
        {
            AutoResetEvent sync = new AutoResetEvent(false);
            CyApiErr err = CyApiErr.OK;

            // Set the connection handler
            m_bleMgrCb.ConnectionHandler = (result, status) =>
            {
                if (status == CyStatus.BLE_STATUS_OK)
                {
                    m_peerDevice = result.Device;

                    // Register the device callback
                    m_deviceCb = new DeviceCb();
                    m_peerDevice.RegisterCallback(m_deviceCb);

                    // Register the GATT client callback
                    m_gattClientCb = new GattClientCb();
                    m_peerDevice.GattClient.RegisterCallback(m_gattClientCb);
                    RegisterNotificationLogger();
                }
                else
                {
                    err = new CyApiErr("Failed to connect to the peer device. Reason: " + status.ToString());
                }

                sync.Set();
            };            

            // Initiate connection request to the peer BLE device
            err = BleMgr.Connect(new CyConnectInfo(deviceAddr, new CyBleConnectionSettings()));
            if (err.IsOK)
                sync.WaitOne();

            return err;
        }

        /// <summary>
        /// Start logging
        /// </summary>
        public CyApiErr StartLogging(ushort ccdHandle)
        {
            // Enable notification
            return Write(ccdHandle, BitConverter.GetBytes(ENABLE_NOTIFICATION));
        }

        /// <summary>
        /// Stop logging
        /// </summary>
        public CyApiErr StopLogging(ushort ccdHandle)
        {
            // Disable notification
            return Write(ccdHandle, BitConverter.GetBytes(DISABLE_NOTIFICATION));
        }

        #endregion

        #region private

        /// <summary>
        /// Start or stop monitoring helper
        /// </summary>
        private CyApiErr Write(ushort handle, params byte[] data)
        {
            AutoResetEvent sync = new AutoResetEvent(false);
            CyApiErr err = CyApiErr.OK;

            // Setup the descriptor write handler
            m_gattClientCb.DescriptorWriteHandler = (CyConnectResult, status) =>
            {
                if (status != CyStatus.BLE_STATUS_OK)
                    err = new CyApiErr("Failed to write descriptor. " + status.ToString());

                sync.Set();
            };

            // Initiate write descriptor request to the CCCD
            err = GattClient.WriteDescriptor(new CyGattWriteInfo(handle, data));
            if (err.IsOK)
                sync.WaitOne();

            return err;
        }

        private CyApiErr Read(ushort handle){
            AutoResetEvent sync = new AutoResetEvent(false);
            CyApiErr err = CyApiErr.OK;

            // Setup the descriptor write handler
            m_gattClientCb.DescriptorReadHandler = (CyConnectResult, status) =>
            {
                if (status != CyStatus.BLE_STATUS_OK)
                    err = new CyApiErr("Failed to read descriptor. " + status.ToString());
                Console.WriteLine("Value: {0}", GetIntegerValue(CyConnectResult.Value));
                dvm.StackAddress = "" + GetIntegerValue(CyConnectResult.Value);
                sync.Set();
            };

            // Initiate write descriptor request to the CCCD
            err = GattClient.ReadDescriptor(new CyGattReadInfo(handle));
            if (err.IsOK)
                sync.WaitOne();

            return err;
        }

        /// <summary>
        /// Register notification logger
        /// </summary>
        private void RegisterNotificationLogger()
        {
            // Setup the characteristic changed handler
            m_gattClientCb.CharacteristicChangedHandler = (info) =>
            {   
                string notif_src;
                switch(info.Handle){
                    case 0x0012:
                        notif_src = "Temperature";
                        dvm.Temperature = "" + GetIntegerValue(info.Value);
                        break;
                    case 0x001A:
                        notif_src = "Pressure";
                        dvm.Pressure = "" + GetIntegerValue(info.Value);
                        break;
                    default:
                        notif_src = "Handle "+info.Handle;
                        break;
                }
                // Log all received notifications
                Console.WriteLine("[{0}] {1}: {2}",
                    DateTime.Now.ToString("HH:mm:ss:fff"),
                    notif_src,
                    GetIntegerValue(info.Value));

            };
        }

        /// <summary>
        /// Gets a printable string from the value byte array
        /// </summary>
        private string GetPrintableValue(params byte[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in value)
                sb.AppendFormat("{0} ", item.ToString("X2"));

            return sb.ToString().Trim();
        }
        private int GetIntegerValue(params byte[] val)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(val);
            switch(val.Length){
                case 1:
                    return (int) val[0];
                case 2:
                    return BitConverter.ToInt16(val, 0);
                case 4:
                    return BitConverter.ToInt16(val, 0);
                default:
                    Console.WriteLine("Couldn't convert. Array length: {0}", val.Length);
                    return -1;
            }
        }
        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose the notification monitor instance
        /// </summary>
        public void Dispose()
        {
            if(m_communicator != null)
            {
                CyApiErr err = CySmartDongleMgr.GetInstance().CloseCommunicator(m_communicator);
                if (err.IsNotOK)
                    Console.WriteLine(err.Message);

                m_communicator = null;
            }
        }

        #endregion

        #region Main

        public CyApiErr connect(string com) {
            CyApiErr err = this.ConnectToDongle(new CyDongleInfo(com, CyDongleInfo.CySmartDongleType.CY5670));
            if (err.IsNotOK)
            {
               return err;
            }
            Console.WriteLine("Connecting to peer device: [0x{0:X12},{1}] ...", PEER_DEVICE_BD_ADDR.Address, PEER_DEVICE_BD_ADDR.AddressType);
            err = this.ConnectToBleDevice(PEER_DEVICE_BD_ADDR);

            return err;
        }

        public void disconnect()
        {
            Dispose();
        }

        public CyApiErr turnOnTempNotifs() {
            Console.WriteLine("Turning on temp notifications");
            return this.StartLogging(TEMPERATURE_CCCD_HANDLE);
        }
        public CyApiErr turnOffTempNotifs()
        {
            Console.WriteLine("Turning off temp notifications");
            return this.StopLogging(TEMPERATURE_CCCD_HANDLE);
        }
        public CyApiErr turnOnPressureNotifs()
        {
            Console.WriteLine("Turning on pressure notifications");
            return this.StartLogging(PRESSURE_CCCD_HANDLE);
        }
        public CyApiErr turnOffPressureNotifs()
        {
            Console.WriteLine("Turning off pressure notifications");
            return this.StopLogging(PRESSURE_CCCD_HANDLE);
        }
        public CyApiErr readAddress()
        {
            return this.Read(STACK_ADDRESS_HANDLE);
        }
        public CyApiErr updateSetpoint(int setpoint)
        {
            return this.Write(TEMPERATURE_HANDLE, BitConverter.GetBytes(setpoint));
        }
        public CyApiErr turnOnThermoController()
        {
            return this.Write(TEMP_CONTROLL_ONOFF_HANDLE, BitConverter.GetBytes(0x01));
        }
        public CyApiErr turnOffThermoController()
        {
            return this.Write(TEMP_CONTROLL_ONOFF_HANDLE, BitConverter.GetBytes(0x00));
        }
        public CyApiErr updateMotorSpeed(int speed)
        {
            return this.Write(MOTOR_SPEED_HANDLE, BitConverter.GetBytes(speed));
        }
        public CyApiErr hapticPreset(int preset)
        {
            return this.Write(HAPTIC_PRESET_HANDLE, BitConverter.GetBytes(preset));
        }
        /*
        static void Main(string[] args)
        {
            using (NotificationLogger logger = new NotificationLogger())
            {
                Console.WriteLine("Connecting to the dongle at {0} ...", COM_PORT_NAME);

                // Change the dongle type to match with the dongle you need to connect
                CyApiErr err = logger.ConnectToDongle(new CyDongleInfo(COM_PORT_NAME, CyDongleInfo.CySmartDongleType.CY5670));
                if (err.IsNotOK)
                {
                    Console.WriteLine(err.Message);
                    return;
                }

                Console.WriteLine("Connecting to peer device: [0x{0:X12},{1}] ...", PEER_DEVICE_BD_ADDR.Address, PEER_DEVICE_BD_ADDR.AddressType);
                err = logger.ConnectToBleDevice(PEER_DEVICE_BD_ADDR);
                if (err.IsNotOK)
                {
                    Console.WriteLine(err.Message);
                    return;
                }

                Console.WriteLine("type a command");
                while(true) {
                    string command = Console.ReadLine();
                    string[] parts = command.Split();
                    switch(parts[0]){
                        case "T": //Temp notifs ON
                            Console.WriteLine("Turning on temp notifications");
                            err = logger.StartLogging(TEMPERATURE_CCCD_HANDLE);
                            break;
                        case "t": //Temp notifs OFF
                            Console.WriteLine("Turning off temp notifications");
                            err = logger.StopLogging(TEMPERATURE_CCCD_HANDLE);
                            break;
                        case "P": //Pressure notifs ON
                            Console.WriteLine("Turning on pressure notifications");
                            err = logger.StartLogging(PRESSURE_CCCD_HANDLE);
                            break;
                        case "p": //Pressure notifs OFF
                            Console.WriteLine("Turning off pressure notifications");
                            err = logger.StopLogging(PRESSURE_CCCD_HANDLE);
                            break;
                        case "O": //Temp controller ON
                            Console.WriteLine("Turning on temperature controller");
                            err = logger.Write(TEMP_CONTROLL_ONOFF_HANDLE, BitConverter.GetBytes(0x01));
                            break;
                        case "o": //Temp controller OFF
                            Console.WriteLine("Turning off temperature controller");
                            err = logger.Write(TEMP_CONTROLL_ONOFF_HANDLE, BitConverter.GetBytes(0x00));
                            break;
                        case "a": //Read stack address
                            Console.WriteLine("Reading stack address...");
                            err = logger.Read(STACK_ADDRESS_HANDLE);
                            break;
                        case "hp": //Write Haptic Preset
                            int val  = Int32.Parse(parts[1]);
                            Console.WriteLine("Writing {0} to Haptic Preset", val);
                            logger.Write(HAPTIC_PRESET_HANDLE, BitConverter.GetBytes(val));
                            break;
                        case "hb": //Write Haptic Buzz
                            val  = Int32.Parse(parts[1]);
                            Console.WriteLine("Writing {0} to Haptic Buzz", val);
                            logger.Write(HAPTIC_BUZZ_HANDLE, BitConverter.GetBytes(val));
                            break;
                        case "m":
                            val  = Int32.Parse(parts[1]);
                            Console.WriteLine("Writing {0} to Motor Speed", val);
                            logger.Write(MOTOR_SPEED_HANDLE, BitConverter.GetBytes(val));
                            break;
                        case "e":
                            logger.Dispose();
                            return;
                        default:
                            Console.WriteLine("unknown command");
                            break;
                        
                    } 
                    if (err.IsNotOK) {
                            Console.WriteLine(err.Message);
                            logger.Dispose();
                            return;
                        } 
                }              

            }
            
        }
        */


        #endregion
    }

    #endregion

    #region BleMgrCb

    /// <summary>
    /// BLE manager Callback class
    /// </summary>
    class BleMgrCb : CyBleMgrCallback
    {
        /// <summary>
        /// Gets/Sets the connection handler
        /// </summary>
        public CySmart.Common.Base.Compatibility.Net20.Action<CyConnectResult, CyStatus> ConnectionHandler { get; set; }

        public override void OnConnected(CyConnectResult result, CyStatus status)
        {
            if (ConnectionHandler != null)
                ConnectionHandler(result, status);
        }
    }

    #endregion

    #region DeviceCb

    /// <summary>
    /// Device callback class
    /// </summary>
    class DeviceCb : CyBleDeviceCallback
    {
        // override callback methods if you need to support pairing.
        // Refer to the CySmart API reference guide
    }

    #endregion

    #region GattClientCb

    /// <summary>
    /// GATT client callback class
    /// </summary>
    class GattClientCb : CyGattClientCallback
    {
        /// <summary>
        /// Gets/Sets the descriptor write handler
        /// </summary>
        public CySmart.Common.Base.Compatibility.Net20.Action<CyGattWriteResult, CyStatus> DescriptorWriteHandler { get; set; }
        public CySmart.Common.Base.Compatibility.Net20.Action<CyGattReadResult, CyStatus> DescriptorReadHandler { get; set; }

        /// <summary>
        /// Gets/Sets the characteristic changed handler
        /// </summary>
        public Action<CyCharacteristicChangedInfo> CharacteristicChangedHandler { get; set; }

        public override void OnDescriptorWrite(CyGattWriteResult result, CyStatus status)
        {
            if (DescriptorWriteHandler != null)
                DescriptorWriteHandler(result, status);
        }

        public override void OnDescriptorRead(CyGattReadResult result, CyStatus status)
        {
            if (DescriptorReadHandler != null)
                DescriptorReadHandler(result, status);
        }

        public override void OnCharacteristicChanged(CyCharacteristicChangedInfo info)
        {
            if (CharacteristicChangedHandler != null)
                CharacteristicChangedHandler(info);
        }
    }

    #endregion
}
