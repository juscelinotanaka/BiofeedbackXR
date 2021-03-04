using System;
using System.Collections.Generic;
using BLEServices;
using UnityEngine;
using VisualGraphs;

public class MyoBLE : MonoBehaviour
{
    private List<BLEService.Characteristic> characteristics = new List<BLEService.Characteristic>();
    private List<BLEService.Device> devices = new List<BLEService.Device>();

    private string _debugText = "-";

    public Transform controller;

    [SerializeField] private Quaternion quaternionMultiplier = new Quaternion(1, 1, 1, 1);
    [SerializeField] private Vector3 correctionEuler;


    public LineChart[] charts;

    public SphereMover SphereMover;
    private bool _hasSphereMover;

    private BLEService.Device _connectedDevice;

    private void Awake()
    {
        _hasSphereMover = SphereMover != null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AddNumbers();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SubNumbers();
        }
    }

    private void SubNumbers()
    {
        a--;
        if (a == -1)
        {
            a = 3;
            b--;
            if (b == -1)
            {
                b = 3;
                c--;
                if (c == -1)
                {
                    c = 3;
                }
            }
        }

        UpdateEuler();
    }

    private int a, b, c;

    private void AddNumbers()
    {
        a++;
        if (a == 4)
        {
            a = 0;
            b++;
            if (b == 4)
            {
                b = 0;
                c++;
                if (c == 4)
                    c = 0;
            }
        }

        UpdateEuler();
    }

    private void UpdateEuler()
    {
        correctionEuler.x = IntToAngle(a);
        correctionEuler.y = IntToAngle(b);
        correctionEuler.z = IntToAngle(c);
    }

    private float IntToAngle(int i)
    {
        switch (i)
        {
            case 1:
                return 90;
            case 2:
                return 180;
            case 3:
                return 270;
            default:
                return i;
        }
    }

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

                case BLEService.State.Scanning:
                    PrintAvailableDevicesList();
                    break;

                case BLEService.State.Connected:
                    PrintCommands();

                    ReadMyoInfoButton();

                    ReadImuDataButton();
                    ReadEmgDataButton();
                    break;

                case BLEService.State.Communicating:
                    PrintCommands();
                    GUILayout.Label($"Data: {_debugText}");
                    break;

                case BLEService.State.Disconnected:
                    if (characteristics.Count > 0)
                        characteristics.Clear();
                    PrintAvailableDevicesList();
                    break;
            }
        }
    }

    private const string MyoInfoCharacteristic = "D5060101-A904-DEB9-4748-2C7F4A124842";

    private void ReadMyoInfoButton()
    {
        if (GUILayout.Button("Read Myo Info"))
        {
            var characteristic = new BLEService.Characteristic()
            {
                Device = _connectedDevice,
                ServiceUuid = ControlService,
                CharacteristicUuid = MyoInfoCharacteristic
            };
            Debug.Log("requesting..");

            // BLEService.ReadCharacteristic(characteristic,
            //     (s, bytes) =>
            //     {
            //         Debug.Log("Something received");
            //         _debugText = $"{Time.realtimeSinceStartup}: {s} - {bytes.Length}";
            //     });
        }
    }

    private void PrintCommands()
    {
        Command("Vibrate", new byte[] {0x0b});
        GUILayout.BeginHorizontal();
        Command("Lock", new byte[] {0x0a, 0x00});
        Command("Unlock Timed", new byte[] {0x0a, 0x01});
        Command("Unlock Always", new byte[] {0x0a, 0x02});
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Sleep Mode: ");
        Command("Normal", new byte[] {0x09, 0x00});
        Command("Never Sleep", new byte[] {0x09, 0x01});
        GUILayout.EndHorizontal();
        GUILayout.Label("Modes");
        GUILayout.BeginHorizontal();
        Command("EMG + IMU + Classif.", new byte[] {0x01, 0x02, 0x01, 0x01});
        Command("EMG + IMU", new byte[] {0x01, 0x03, 0x02, 0x01, 0x00});
        Command("EMG", new byte[] {0x01, 0x03, 0x02, 0x00, 0x00});
        Command("IMU", new byte[] {0x01, 0x03, 0x00, 0x01, 0x00});
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    private void Command(string action, byte[] data)
    {
        if (GUILayout.Button(action))
        {
            SendCommand(data);
        }
    }

    private const string ControlService = "D5060001-A904-DEB9-4748-2C7F4A124842";
    private const string CommandCharacteristic = "D5060401-A904-DEB9-4748-2C7F4A124842";

    private void SendCommand(byte[] data)
    {
        var characteristic = new BLEService.Characteristic()
        {
            CharacteristicUuid = CommandCharacteristic,
            Device = _connectedDevice,
            ServiceUuid = ControlService
        };

        BLEService.WriteToCharacteristic(characteristic,
            data, false, s1 => { Debug.Log($"Reply: {s1}"); });
    }


    private const string ImuService = "D5060002-A904-DEB9-4748-2C7F4A124842";
    private const string ImuDataCharacteristic = "D5060402-A904-DEB9-4748-2C7F4A124842";

    private void ReadImuDataButton()
    {
        if (GUILayout.Button("Read IMU Data"))
        {
            var characteristic = new BLEService.Characteristic
            {
                Device = _connectedDevice,
                ServiceUuid = ImuService,
                CharacteristicUuid = ImuDataCharacteristic
            };

            BLEService.Subscribe(characteristic, ParseIMUData);
        }
    }

    private string _emgService = "D5060005-A904-DEB9-4748-2C7F4A124842";
    private string _emgDataCharacteristic1 = "D5060105-A904-DEB9-4748-2C7F4A124842";
    private string _emgDataCharacteristic2 = "D5060205-A904-DEB9-4748-2C7F4A124842";
    private string _emgDataCharacteristic3 = "D5060305-A904-DEB9-4748-2C7F4A124842";
    private string _emgDataCharacteristic4 = "D5060405-A904-DEB9-4748-2C7F4A124842";

    private void ReadEmgDataButton()
    {
        if (GUILayout.Button("Read EMG Data"))
        {
            var characteristic = new BLEService.Characteristic
            {
                Device = _connectedDevice,
                ServiceUuid = _emgService,
                CharacteristicUuid = _emgDataCharacteristic1
            };
            BLEService.Subscribe(characteristic, data => ParseEMGData(data, 0));

            characteristic.CharacteristicUuid = _emgDataCharacteristic2;
            BLEService.Subscribe(characteristic, data => ParseEMGData(data, 1));

            characteristic.CharacteristicUuid = _emgDataCharacteristic3;
            BLEService.Subscribe(characteristic, data => ParseEMGData(data, 2));

            characteristic.CharacteristicUuid = _emgDataCharacteristic4;
            BLEService.Subscribe(characteristic, data => ParseEMGData(data, 3));
        }
    }

    private float time;
    private float lastTime;
    private int count = -1;
    private float sum = 0;

    private void ParseEMGData(BLEService.Data data, int index)
    {
        lastTime = time;
        time = Time.realtimeSinceStartup;

        float diff = time - lastTime;
        if (count != -1)
        {
            sum += diff;
        }

        count++;

        byte[] rawData = data.RawData;
        // Debug.Log($"Diff: {diff} : {sum / count} : {rawData.Length}");

        var sampleA = new float[]
        {
            rawData[0],
            rawData[1],
            rawData[2],
            rawData[3],
            rawData[4],
            rawData[5],
            rawData[6],
            rawData[7]
        };

        var sampleB = new float[]
        {
            rawData[0],
            rawData[1],
            rawData[2],
            rawData[3],
            rawData[4],
            rawData[5],
            rawData[6],
            rawData[7]
        };

        _debugText = "";
        foreach (float s in sampleA)
        {
            _debugText += s.ToString("000") + ", ";
        }

        Debug.Log(_debugText);

        PlotOnCharts(sampleB);
    }

    private void PrintAvailableDevicesList()
    {
        foreach (var device in devices)
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

        var sdata = new short[(int) Mathf.Ceil(bytes.Length / 2f)];
        Buffer.BlockCopy(bytes, 0, sdata, 0, bytes.Length);

        var w = (short) (orientationScale * sdata[0]);
        var x = (short) (orientationScale * sdata[1]);
        var y = (short) (orientationScale * sdata[2]);
        var z = (short) (orientationScale * sdata[3]);

        var accx = (short) (accelerometerScale * sdata[4]);
        var accy = (short) (accelerometerScale * sdata[5]);
        var accz = (short) (accelerometerScale * sdata[6]);

        var gx = (short) (gyroscopeScale * sdata[7]);
        var gy = (short) (gyroscopeScale * sdata[8]);
        var gz = (short) (gyroscopeScale * sdata[9]);

        // SetDebugText(bytes, sdata, w, x, y, z, gx, gy, gz, accx, accy, accz);

        _debugText = $"{x:0000} - {z:0000} - {y:0000} - {w:0000} : {(x * x + y * y + z * z + w * w)}";

        PlotOnCharts(accx, accy, accz);

        HandleAcc(accx, accy, accz);

        var rotation = new Quaternion(quaternionMultiplier.x * y, quaternionMultiplier.y * z,
            quaternionMultiplier.z * x, quaternionMultiplier.w * w);
        var finalRotation = Quaternion.Euler(correctionEuler) * rotation;
        controller.rotation = finalRotation;
    }

    private void PlotOnCharts(params float[] data)
    {
        int max = Mathf.Max(data.Length, charts.Length);
        for (var i = 0; i < max; i++)
        {
            if (i >= data.Length || i >= charts.Length)
                continue;

            charts[i].Insert(data[i]);
        }
    }

    private void HandleAcc(short x, short y, short z)
    {
        if (!_hasSphereMover)
            return;

        var move = SphereMover.Move.None;
        if (x > 6000) move = SphereMover.Move.Backward;
        if (x < -6000) move = SphereMover.Move.Forward;
        if (y < -5000) move = SphereMover.Move.Left;
        if (y > 5000) move = SphereMover.Move.Right;
        if (z > 5000) move = SphereMover.Move.Down;
        if (z < -3500) move = SphereMover.Move.Up;

        SphereMover.ApplyMove(move);
    }

    private void SetDebugText(byte[] bytes, short[] sdata, short w, short x, short y, short z, short gx, short gy,
        short gz,
        short accx, short accy, short accz)
    {
        _debugText = $"[{bytes.Length}]: ";
        const string format = "00000";
        foreach (byte b in bytes)
        {
            _debugText += ((int) b).ToString(format);
            _debugText += " ";
        }

        _debugText += $"\n: {sdata.Length}";
        foreach (short s1 in sdata)
        {
            _debugText += s1.ToString(format) + " ";
        }

        _debugText += "\n";
        _debugText += "Orientation  : ";
        _debugText += w.ToString(format) + " ";
        _debugText += x.ToString(format) + " ";
        _debugText += y.ToString(format) + " ";
        _debugText += z.ToString(format) + " ";
        _debugText += "\n";
        _debugText += "Gyroscope    : ";
        _debugText += gx.ToString(format) + " ";
        _debugText += gy.ToString(format) + " ";
        _debugText += gz.ToString(format) + " ";
        _debugText += "\n";
        _debugText += "Accelerometer: ";
        _debugText += accx.ToString(format) + " ";
        _debugText += accy.ToString(format) + " ";
        _debugText += accz.ToString(format) + " ";
    }
}