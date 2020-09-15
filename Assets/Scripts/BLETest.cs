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
                BLEService.Initialize(() => Debug.Log("Initialized"), error => Debug.Log($"Error: {error}"));
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
                            Debug.Log($"Device found: {device.Name}");
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
                    
                    break;
                case BLEService.State.Subscribing:
                    break;
                case BLEService.State.Communicating:
                    GUILayout.Label($"Data: {s}");
                    break;
                case BLEService.State.Disconnected:
                    PrintAvailableDevicesList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void PrintAvailableDevicesList()
    {
        foreach (BLEService.Device device in devices)
        {
            if (GUILayout.Button(device.Name))
            {
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
            s = $"-{bytes.Length}-";
            foreach (byte b in bytes)
            {
                s += (int) b;
                s += " ";
            }
        }
    }
}
