using System;
using System.Collections.Generic;
using BLEServices;
using UnityEngine;

public class BLETest : MonoBehaviour
{
    private List<BLEService.Characteristic> characteristics = new List<BLEService.Characteristic>();
    private List<BLEService.Device> devices = new List<BLEService.Device>();
    private string s = "-";

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
                    foreach (BLEService.Characteristic characteristic in characteristics)
                    {
                        if (GUILayout.Button($"{characteristic.Device.Name}: {characteristic.CharacteristicUuid}"))
                        {
                            BLEService.Subscribe(characteristic, OnDataReceived);
                        }
                    }

                    SubscribeToAnyCharacteristic();

                    WriteToCharacteristic();

                    break;
                case BLEService.State.Subscribing:
                    break;
                case BLEService.State.Communicating:
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

    private void WriteToCharacteristic()
    {
        GUILayout.Label("Write to Characteristic");
        if (GUILayout.Button("WRITE"))
        {
            BLEService.Characteristic characteristic = new BLEService.Characteristic()
            {
                CharacteristicUuid = _characteristic,
                Device = _connectedDevice,
                ServiceUuid = _service
            };

            // byte[] data = new byte[] {0x01, 0x00, 0x01, 0x00}; // vibrate long
            byte[] data = new byte[] {0x03, 0x03}; // start IMU
            BLEService.WriteToCharacteristic(characteristic, 
                data, false, s1 =>
            {
                Debug.Log($"Reply: {s1}");
            });
        }
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

            BLEService.Subscribe(characteristic, OnDataReceived);
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

    private void OnDataReceived(BLEService.Data data)
    {

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
            s = $"f-{bytes.Length}-";
            foreach (byte b in bytes)
            {
                s += (int) b;
                s += " ";
            }
        }
    }
}
