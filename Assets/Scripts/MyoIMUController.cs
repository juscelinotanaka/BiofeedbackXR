using System;
using System.Collections;
using System.Text;
using BLEServices;
using TMPro;
using UnityEngine;
using VisualGraphs;

public class MyoIMUController : MonoBehaviour
{
    private string _debugText = "-";
    private StringBuilder _builder = new StringBuilder(500);
    public TextMeshPro debugText;
    public Transform controller;

    Quaternion quaternion = Quaternion.Euler(new Vector3(0, 90, 0));

    public LineChart[] charts;

    private static BLEService.Device _connectedDevice;
    private bool _hasError;

    public Spaceship spaceship;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2);

        if (!BLEService.IsInitialized)
        {
            BLEService.Initialize(
                () =>
                {
                    BLEService.StartScan(device =>
                    {
                        if (device.Name == "Myo HSD")
                        {
                            _connectedDevice = device;
                            BLEService.ConnectToDevice(device,
                                characteristic => { StartCoroutine(SetupMyoAndSubscribe()); },
                                disconnectedAddress => { SetError($"Device Disconnected: {disconnectedAddress}"); });
                        }
                    });
                },
                error => SetError($"Initialization -> {error}")
            );
        }
    }

    private void SetError(string error)
    {
        _hasError = true;
        _builder.Append($"ERROR: {error}");
        _builder.Append($"ERROR STATE: {BLEService.CurrentState.ToString()}");

        StartCoroutine(CountdownError());
    }

    private IEnumerator CountdownError()
    {
        yield return new WaitForSeconds(1);
        _hasError = false;
    }

    public static void Vibrate()
    {
        Command("Vibrate", new byte[] {0x0b});
    }

    private IEnumerator SetupMyoAndSubscribe()
    {
        Command("Vibrate", new byte[] {0x0b});
        yield return new WaitForSeconds(0.75f);
        Command("Unlock Always", new byte[] {0x0a, 0x02});
        yield return new WaitForSeconds(0.75f);
        Command("Never Sleep", new byte[] {0x09, 0x01});
        yield return new WaitForSeconds(0.75f);
        Command("IMU", new byte[] {0x01, 0x03, 0x00, 0x03, 0x00});
        yield return new WaitForSeconds(0.75f);

        var characteristic = new BLEService.Characteristic
        {
            Device = _connectedDevice,
            ServiceUuid = ImuService,
            CharacteristicUuid = ImuDataCharacteristic
        };

        BLEService.Subscribe(characteristic, ParseIMUData);

        yield return new WaitForSeconds(0.75f);

        characteristic = new BLEService.Characteristic
        {
            Device = _connectedDevice,
            ServiceUuid = ImuService,
            CharacteristicUuid = ImuEventCharacteristic
        };

        BLEService.Subscribe(characteristic, ParseIMUEvent);
    }

    private void ParseIMUEvent(BLEService.Data data)
    {
        byte[] bytes = data.RawData;

        var type = bytes[0];
        var tapDirection = bytes[1];
        var tapCount = bytes[2];

        SetError($"Event: {type} - {tapDirection} - {tapCount}");
    }

    private void Update()
    {
        spaceship.SetConneted(BLEService.CurrentState == BLEService.State.Communicating);

        if (!_hasError)
        {
            _builder.Clear();
            _builder.Append(BLEService.CurrentState.ToString());
        }


        debugText.text = _builder.ToString();
    }

    private static void Command(string action, byte[] data)
    {
        SendCommand(data);
    }

    private const string ControlService = "D5060001-A904-DEB9-4748-2C7F4A124842";
    private const string CommandCharacteristic = "D5060401-A904-DEB9-4748-2C7F4A124842";

    private static void SendCommand(byte[] data)
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
    private const string ImuEventCharacteristic = "D5060502-A904-DEB9-4748-2C7F4A124842";

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

        PlotOnCharts(accx, accy, accz);

        var rotation = new Quaternion(y, z, -x, -w);
        var finalRotation = quaternion * rotation;
        controller.rotation = finalRotation;

        // if (x > 6000) move = SphereMover.Move.Backward;
        // if (x < -6000) move = SphereMover.Move.Forward;
        // if (y < -5000) move = SphereMover.Move.Left;
        // if (y > 5000) move = SphereMover.Move.Right;
        // if (z > 5000) move = SphereMover.Move.Down;
        // if (z < -3500) move = SphereMover.Move.Up;
        // bool thrust = Mathf.Abs(x) > 6000 || Mathf.Abs(y) > 5000;
        // if (thrust)
        //     spaceship.Thrust();
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