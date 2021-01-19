using System;
using System.Collections.Generic;
using BLEServices;
using UnityEngine;
using VisualGraphs;

public class MyoBLE : MonoBehaviour
{
    private List<BLEService.Characteristic> characteristics = new List<BLEService.Characteristic>();
    private List<BLEService.Device> devices = new List<BLEService.Device>();
    private string s = "-";

    public Transform controller;

    public LineChart xChart;
    public LineChart yChart;
    public LineChart zChart;

    public SphereMover SphereMover;

    private void OnGUI()
    {
        GUI.skin.button.fontSize = 40;
        GUI.skin.label.fontSize = 40;

        if (!BLEService.IsInitialized)
        {
            if (GUILayout.Button("Initialize"))
                BLEService.Initialize(
                    () => Debug.Log("Initialized"),
                    error => Debug.Log($"Error: {error}")
                );
        }
        else
        {
            GUILayout.Label($"State: {BLEService.CurrentState}");
            switch (BLEService.CurrentState)
            {
                case BLEService.State.ReadyToScan:
                    if (GUILayout.Button("Scan"))
                    {
                        BLEService.StartScan(device =>
                        {
                            if (!devices.Contains(device))
                            {
                                devices.Add(device);
                            }
                        });
                    }

                    break;
                case BLEService.State.Disconnecting:
                    break;
                case BLEService.State.Scanning:
                    PrintAvailableDevicesList();
                    break;
                case BLEService.State.Connecting:
                    break;
                case BLEService.State.Connected:
                    PrintCommands();

                    ReadData();
                    break;
                case BLEService.State.Subscribing:
                    break;
                case BLEService.State.Communicating:
                    PrintCommands();
                    GUILayout.Label($"Data: {s}");
                    break;
                case BLEService.State.Disconnected:
                    if (characteristics.Count > 0)
                        characteristics.Clear();
                    PrintAvailableDevicesList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private const string CommandService = "D5060001-A904-DEB9-4748-2C7F4A124842";
    private const string CommandCharacteristic = "D5060401-A904-DEB9-4748-2C7F4A124842";

    private const string ImuService = "D5060002-A904-DEB9-4748-2C7F4A124842";
    private const string ImuDataCharacteristic = "D5060402-A904-DEB9-4748-2C7F4A124842";

    // private string _emgService = "D5060005-A904-DEB9-4748-2C7F4A124842";
    // private string _emgDataCharacteristic1 = "D5060105-A904-DEB9-4748-2C7F4A124842";
    // private string _emgDataCharacteristic2 = "D5060205-A904-DEB9-4748-2C7F4A124842";
    // private string _emgDataCharacteristic3 = "D5060305-A904-DEB9-4748-2C7F4A124842";
    // private string _emgDataCharacteristic4 = "D5060405-A904-DEB9-4748-2C7F4A124842";

    private void ReadData()
    {
        if (GUILayout.Button("Read Data"))
        {
            BLEService.Characteristic characteristic = new BLEService.Characteristic
            {
                Device = _connectedDevice,
                ServiceUuid = ImuService,
                CharacteristicUuid = ImuDataCharacteristic
            };

            BLEService.Subscribe(characteristic, ParseIMUData);
        }
    }

    private void PrintCommands()
    {
        Command("Vibrate", new byte[] {0x0b});
        GUILayout.BeginHorizontal();
        Command("Lock", new byte[] {0x0a, 0x00});
        Command("Unlock Timed", new byte[] {0x0a, 0x01});
        Command("Unlock 4ever", new byte[] {0x0a, 0x02});
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        Command("Sleep Mode Normal", new byte[] {0x09, 0x00});
        Command("Sleep Mode Never Sleep", new byte[] {0x09, 0x01});
        GUILayout.EndHorizontal();
        GUILayout.Label("Modes");
        GUILayout.BeginHorizontal();
        Command("EMG + IMU + Classif.", new byte[] {0x01, 0x02, 0x01, 0x01});
        Command("EMG + IMU", new byte[] {0x01, 0x02, 0x01, 0x00});
        Command("EMG", new byte[] {0x01, 0x02, 0x00, 0x00});
        Command("IMU", new byte[] {0x01, 0x00, 0x00, 0x01});
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    public void Command(string action, byte[] data)
    {
        if (GUILayout.Button(action))
        {
            SendCommand(data);
        }
    }

    private void SendCommand(byte[] data)
    {
        BLEService.Characteristic characteristic = new BLEService.Characteristic()
        {
            CharacteristicUuid = CommandCharacteristic,
            Device = _connectedDevice,
            ServiceUuid = CommandService
        };

        BLEService.WriteToCharacteristic(characteristic,
            data, false, s1 => { Debug.Log($"Reply: {s1}"); });
    }

    private string _service = "D5060001-A904-DEB9-4748-2C7F4A124842";
    private string _characteristic = "D5060401-A904-DEB9-4748-2C7F4A124842";
    private BLEService.Device _connectedDevice;

    private void SubscribeToAnyCharacteristic()
    {
        _characteristic = GUILayout.TextField(_characteristic);
        _service = GUILayout.TextField(_service);
        if (GUILayout.Button("Subscribe to the given one"))
        {
            BLEService.Characteristic characteristic = new BLEService.Characteristic()
            {
                CharacteristicUuid = _characteristic,
                Device = _connectedDevice,
                ServiceUuid = _service
            };

            BLEService.Subscribe(characteristic, ParseIMUData);
        }
    }

    private void PrintAvailableDevicesList()
    {
        foreach (BLEService.Device device in devices)
        {
            if (GUILayout.Button(device.Name))
            {
                _connectedDevice = device;
                BLEService.ConnectToDevice(device,
                    characteristic => { characteristics.Add(characteristic); },
                    disconnectedAddress => { Debug.Log($"Device Disconnected: {disconnectedAddress}"); });
            }
        }
    }


    private void ParseIMUData(BLEService.Data data)
    {
        const float orientationScale = 1.0f;
        const float gyroscopeScale = 1f;
        const float accelerometerScale = 1f;

        byte[] bytes = data.RawData;

        if (bytes.Length == 8)
        {
            uint millis = (uint) ((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
            ushort ecg = (ushort) ((bytes[4] << 8) | bytes[5]);
            ushort emg = (ushort) ((bytes[6] << 8) | bytes[7]);

            s = $"{millis} : {ecg} : {emg}";
        }
        else
        {
            s = $"[{bytes.Length}]: ";
            const string format = "00000";
            foreach (byte b in bytes)
            {
                s += ((int) b).ToString(format);
                s += " ";
            }

            short[] sdata = new short[(int) Mathf.Ceil(bytes.Length / 2f)];
            Buffer.BlockCopy(bytes, 0, sdata, 0, bytes.Length);

            s += $"\n: {sdata.Length}";
            foreach (short s1 in sdata)
            {
                s += s1.ToString(format) + " ";
            }

            short w = (short) (orientationScale * sdata[0]);
            short x = (short) (orientationScale * sdata[1]);
            short y = (short) (orientationScale * sdata[2]);
            short z = (short) (orientationScale * sdata[3]);

            short accx = (short) (accelerometerScale * sdata[4]);
            short accy = (short) (accelerometerScale * sdata[5]);
            short accz = (short) (accelerometerScale * sdata[6]);

            short gx = (short) (gyroscopeScale * sdata[7]);
            short gy = (short) (gyroscopeScale * sdata[8]);
            short gz = (short) (gyroscopeScale * sdata[9]);

            s += "\n";
            s += "Orientation  : ";
            s += w.ToString(format) + " ";
            s += x.ToString(format) + " ";
            s += y.ToString(format) + " ";
            s += z.ToString(format) + " ";
            s += "\n";
            s += "Gyroscope    : ";
            s += gx.ToString(format) + " ";
            s += gy.ToString(format) + " ";
            s += gz.ToString(format) + " ";
            s += "\n";
            s += "Accelerometer: ";
            s += accx.ToString(format) + " ";
            s += accy.ToString(format) + " ";
            s += accz.ToString(format) + " ";

            xChart.Insert(accx);
            yChart.Insert(accy);
            zChart.Insert(accz);

            HandleAcc(accx, accy, accz);

            // controller.rotation = Quaternion.SlerpUnclamped(controller.rotation, new Quaternion(x, y, z, w), Time.deltaTime * 5);
            controller.rotation = new Quaternion(x, y, z, w);
        }
    }

    private void HandleAcc(short x, short y, short z)
    {
        var move = SphereMover.Move.None;
        if (x > 6000) move = SphereMover.Move.Backward;
        if (x < -6000) move = SphereMover.Move.Forward;
        if (y < -5000) move = SphereMover.Move.Left;
        if (y > 5000) move = SphereMover.Move.Right;
        if (z > 5000) move = SphereMover.Move.Down;
        if (z < -3500) move = SphereMover.Move.Up;

        SphereMover.ApplyMove(move);
    }
}