# BLE reactor interface

## Running the Program
- Run `bin/release/BLE.exe`
- Choose the port which the CySmart dongle is connected to.
- Choose the PSoC you are commuicating with. Numbers in the dropdown list correspond to the serial number on the QR code on the PSoCs.
- Make sure the dongle is plugged in and the PSoC is on.
- Click "Connect"
- Click "Receive Temperature Notifications" and "Receive Pressure Notifications" to get temperature and pressure readings.
- Click "Read Address" to get the 5-bit address specified by the jaw PCB.
- Click "Thermocontroller" to set the PSoC to the temperature specified by the setpoint slider.

## Multiple Connections
- Open up multiple instances of `BLE.exe`
- Make sure you have a CySmart dongle for each connection.
- Set the "PSoC" field to correspond to the different reactors.
- Set the "Dongle Port" field to the different dongle ports. Remember, each instance needs a different dongle.
- Click "Connect" for all instances.

## Configuring the Program
- Whenever a change is made, the program needs to be recompiled.
- The easiest way to do this is to download Visual Studio and open up `BLE.sln`
- On the toolbar on the top of Visual Studio, make sure the "Solution Configurations" dropdown says "Release" and not "Debug", and then hit the "Start" button to the right. This will recomplile the project to incorporate a new change.

### Adding a Reactor
- Open up `Donle.cs`. 
- Towards the top of the `Dongle` class there should be declarations of new `CyBleBdAddress` objects. It should look something like this:
```cs
static readonly CyBleBdAddress PEER_DEVICE_BD_ADDR_00795 = new CyBleBdAddress(0x00A050A47A8D, CyBleBdAddressType.PUBLIC_ADDRESS);
static readonly CyBleBdAddress PEER_DEVICE_BD_ADDR_00392 = new CyBleBdAddress(0x00A050E17C5F, CyBleBdAddressType.PUBLIC_ADDRESS);
```
- Add a new line for the new PSoC. It should look like this:
```cs
static readonly CyBleBdAddress PEER_DEVICE_BD_ADDR_<name> = new CyBleBdAddress(<address>, CyBleBdAddressType.PUBLIC_ADDRESS);
```
where `<name>` should correspond to the serial number on the QR code and `<address>` is the 48-bit BLE address unique to each PSoC. This address can be discovered by turning on the PSoC, then running a scan with CySmsart application. It should display the address.
- Navigate to the `Dongle` constructor method (the region name is `ctor`). Add a new entry to the `bluetooth_address` Dictionary. It should look like this:
```cs
bluetooth_address.Add("<name>", PEER_DEVICE_BD_ADDR_<name>);
```
where `<name>` is what you called the PSoC earlier.
- Navigate to `MainWindow.xaml`
- Find the element called `<ComboBox x:Name="psocComboBox">`.
- Add a new item to the element. It should look like this:
```xaml
<ComboBoxItem Name="psoc_<name>"><name></ComboBoxItem>
```
where, again, `<name>` should be the name used earlier.
- Last but not least, open up `DongleViewModel.cs`.
- towards the top of the class there should be a declaration for a new Dictionary called `database_psoc_names`.
Add a new entry like this:
```cs
database_psoc_names.Add("<name>", "<db_addr>");
```
where `<name>` is the name being used and `<db_addr>` is the identifier for the row in the database for the PSoC. For example, if `db_addr>` is `"1"`, then the temperature will be written to `temp_1` and the pressure will be written to `pres_1`.
