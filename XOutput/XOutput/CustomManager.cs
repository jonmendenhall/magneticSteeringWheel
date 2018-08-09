using System;
using System.Threading;
using System.Windows.Forms;
using SlimDX.DirectInput;
using System.Runtime.InteropServices;
using System.IO.Ports;



/// <summary>
/// A big thanks goes to Eric L. Barrett for writing the original XOutput code!
/// 
/// Most of this code is adapted from the XOutput project on GitHub.
/// You can find it here: https://github.com/Stents-/XOutput
/// 
/// I've modified it to fit my need for this project, but the base level code is still the same.
/// </summary>



class CustomManager : ScpDevice
{

    private const String BUS_CLASS_GUID = "{F679F562-3164-42CE-A4DB-E7DDBE723909}";

    private Control handle;
    private DirectInput directInput;
    private XboxOutput controller;
    private Byte[] reportOutput = new Byte[8];
    private SerialPort serial;

    public CustomManager(Control handle): base(BUS_CLASS_GUID)
    {
        this.handle = handle;
        directInput = new DirectInput();
    }

    private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {

            // read one line of data from the serial port;
            string line = serial.ReadLine();

            // split the line into each part... "WHEEL" "0.00" "0" "0" 
            string[] parts = line.Trim().Split(' ');

            // make sure there are 4 parts of data
            if (parts.Length == 4)
            {

                // get the wheel's angle out of 90 degress (flipped)
                double wheel = double.Parse(parts[1]) / -90;

                // make sure it's only -90 to +90
                wheel = Math.Min(Math.Max(wheel, -1), 1);

                // this is a form of the exponential curve used in RC drones and planes. It maps an input value to a curve rather than linear.
                double ek = 0.9;
                wheel = (Math.Pow(wheel, 3) * (ek - 1) + wheel) / ek;

                // this line is specifically for Forza 6. I noticed that the steering seems to always have a 20% inner deadzone, so I just move the values farther out to account for it.
                wheel = (wheel > 0 ? 1 : -1) * 0.2 + wheel * 0.8;

                // get the brake and gas pedal values
                double leftPedal = double.Parse(parts[2]) / 500;
                double rightPedal = double.Parse(parts[3]) / 500;

                // set the data for the Xbox controller report
                controller.LX = (int) Math.Round(wheel * 32767);
                controller.L2 = (byte)Math.Round(leftPedal * 255);
                controller.R2 = (byte) Math.Round(rightPedal * 255);

                // write the report
                Report(Translate(controller), reportOutput);
            }
        }
        catch (Exception) { }
       
    }

    public bool detectSteeringWheel()
    {
        // loop through all Serial ports
        foreach (var name in SerialPort.GetPortNames())
        {

            // open the serial port with that name
            serial = new SerialPort(name, 9600);
            serial.Open();

            // wait some time for any data
            Thread.Sleep(50);

            // check if the device has sent any data containing "WHEEL"
            // this would mean it is the steering wheel you built
            string response = serial.ReadExisting();
            if (response.Contains("WHEEL"))
            {
                // add the data listener for this port because it is the wheel, and stop searching
                serial.DataReceived += Serial_DataReceived;
                return true;
            } else
            {
                serial.Close();
            }
        }

        // no steering wheels were found
        return false;
    }

    // called when you press Start
    public override bool Start()
    {
        Open();

        // look for steering wheels
        if(detectSteeringWheel())
        {
            // connect the fake Xbox 360 controller
            Plugin(1);
            return true;
        } else
        {
            // show error that couldn't find a wheel
            MessageBox.Show("Please make sure your wheel is connected to the computer.", "No steering wheel found", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return false;
    }

    // called when you press Stop
    public override bool Stop()
    {
        // close the serial port and disconnect the fake Xbox 360 controller
        serial.Close();
        Unplug(1);
        return true;
    }










    public Byte[] Translate(XboxOutput controller)
    {
        Byte[] Output = new Byte[28];

        Output[0] = 0x1C;
        Output[4] = 0x01;
        Output[9] = 0x14;

        if (controller.Back) Output[10] |= (Byte)(1 << 5); // Back
        if (controller.L3) Output[10] |= (Byte)(1 << 6); // Left  Thumb
        if (controller.R3) Output[10] |= (Byte)(1 << 7); // Right Thumb
        if (controller.Start) Output[10] |= (Byte)(1 << 4); // Start

        if (controller.DpadUp) Output[10] |= (Byte)(1 << 0); // Up
        if (controller.DpadRight) Output[10] |= (Byte)(1 << 3); // Right
        if (controller.DpadDown) Output[10] |= (Byte)(1 << 1); // Down
        if (controller.DpadLeft) Output[10] |= (Byte)(1 << 2); // Left

        if (controller.L1) Output[11] |= (Byte)(1 << 0); // Left  Shoulder
        if (controller.R1) Output[11] |= (Byte)(1 << 1); // Right Shoulder

        if (controller.Y) Output[11] |= (Byte)(1 << 7); // Y
        if (controller.B) Output[11] |= (Byte)(1 << 5); // B
        if (controller.A) Output[11] |= (Byte)(1 << 4); // A
        if (controller.X) Output[11] |= (Byte)(1 << 6); // X

        if (controller.Home) Output[11] |= (Byte)(1 << 2); // Guide     

        Output[12] = controller.L2; // Left Trigger
        Output[13] = controller.R2; // Right Trigger

        Output[14] = (Byte)((controller.LX >> 0) & 0xFF); // LX
        Output[15] = (Byte)((controller.LX >> 8) & 0xFF);

        Output[16] = (Byte)((controller.LY >> 0) & 0xFF); // LY
        Output[17] = (Byte)((controller.LY >> 8) & 0xFF);

        Output[18] = (Byte)((controller.RX >> 0) & 0xFF); // RX
        Output[19] = (Byte)((controller.RX >> 8) & 0xFF);

        Output[20] = (Byte)((controller.RY >> 0) & 0xFF); // RY
        Output[21] = (Byte)((controller.RY >> 8) & 0xFF);

        return Output;
    }

    #region ScpDevice Functions

    public override Boolean Open(int Instance = 0)
    {
        return base.Open(Instance);
    }

    public override Boolean Open(String DevicePath)
    {
        m_Path = DevicePath;
        m_WinUsbHandle = (IntPtr)INVALID_HANDLE_VALUE;

        if (GetDeviceHandle(m_Path))
        {       
            m_IsActive = true;
        }
        return true;
    }

    public Boolean Plugin(Int32 Serial)
    {
        Int32 Transfered = 0;
        Byte[] Buffer = new Byte[16];

        Buffer[0] = 0x10;
        Buffer[1] = 0x00;
        Buffer[2] = 0x00;
        Buffer[3] = 0x00;

        Buffer[4] = (Byte)((Serial >> 0) & 0xFF);
        Buffer[5] = (Byte)((Serial >> 8) & 0xFF);
        Buffer[6] = (Byte)((Serial >> 16) & 0xFF);
        Buffer[7] = (Byte)((Serial >> 24) & 0xFF);

        return DeviceIoControl(m_FileHandle, 0x2A4000, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
    }

    public Boolean Report(Byte[] Input, Byte[] Output)
    {
        Int32 Transfered = 0;
        bool result = DeviceIoControl(m_FileHandle, 0x2A400C, Input, Input.Length, Output, Output.Length, ref Transfered, IntPtr.Zero) && Transfered > 0;
        return result;
    }

    public Boolean Unplug(Int32 Serial)
    {
        Int32 Transfered = 0;
        Byte[] Buffer = new Byte[16];

        Buffer[0] = 0x10;
        Buffer[1] = 0x00;
        Buffer[2] = 0x00;
        Buffer[3] = 0x00;

        Buffer[4] = (Byte)((Serial >> 0) & 0xFF);
        Buffer[5] = (Byte)((Serial >> 8) & 0xFF);
        Buffer[6] = (Byte)((Serial >> 16) & 0xFF);
        Buffer[7] = (Byte)((Serial >> 24) & 0xFF);

        return DeviceIoControl(m_FileHandle, 0x2A4004, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
    }

    #endregion


}

public struct XboxOutput
{
    public Int32 LX, LY, RX, RY;
    public byte L2, R2;
    public bool A, B, X, Y, Start, Back, L1, R1, L3, R3, Home;
    public bool DpadUp, DpadRight, DpadDown, DpadLeft;
}
