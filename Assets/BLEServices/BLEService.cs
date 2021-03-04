using System;
using UniRx;
using UnityEngine;

namespace BLEServices
{
    public static partial class BLEService 
    {
        /// <summary>
        /// Checks whether the service is already initialized or not
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the current state of the BLE Service
        /// </summary>
        public static State CurrentState { get; private set; }

        public static bool IsConnected => CurrentState >= State.Connected;

        public delegate void OnDeviceFound(Device device);
        public delegate void OnCharacteristicFound(Characteristic characteristic);
        public delegate void OnDataReceived(Data data);

        /// <summary>
        /// Initializes the BLE service and returns it's result via callbacks. If successful, you can start scanning
        /// for devices
        /// </summary>
        public static void Initialize(Action onSuccess, Action<string> onError, bool asCentral = true,
            bool asPeripheral = false)
        {
            if (IsInitialized)
            {
                Debug.LogError("BLE Service is already initialized");
                return;
            }

            IsInitialized = true;

            BluetoothLEHardwareInterface.Initialize(asCentral, asPeripheral,
                () =>
                {
                    IsInitialized = true;
                    SetState(State.ReadyToScan);
                    onSuccess?.Invoke();
                },
                error =>
                {
                    BluetoothLEHardwareInterface.Log("Error: " + error);
                    onError?.Invoke(error);
                });

            Observable
                .OnceApplicationQuit()
                .Subscribe(unit => { DeInitialize(); });
        }

        /// <summary>
        /// Scan all BLE devices in range. You can check if the returned device has the name that you're looking
        /// for. The address will be used to start a connection with the device.
        /// </summary>
        public static void StartScan(OnDeviceFound onDeviceFound)
        {
            CheckIfInitialized();

            if (CurrentState != State.ReadyToScan)
                throw new Exception("You can only start a scan when you are Ready to Scan");

            SetState(State.Scanning);
            Observable
                .Timer(TimeSpan.FromSeconds(0.1f))
                .Subscribe(_ =>
                {
                    BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null,
                        (address, name) => onDeviceFound(new Device
                        {
                            Name = name, Address = address
                        }), null, false, false);
                });
        }

        /// <summary>
        /// Connects to a device and gets its characteristic items. You can check if that device has the services + 
        /// characteristics that you are looking for. An error may happen on the way, so check the error callback
        /// </summary>
        public static void ConnectToDevice(Device device, OnCharacteristicFound onCharacteristicFound, Action<string> onDisconnect)
        {
            CheckIfInitialized();

            if (CurrentState != State.Scanning && CurrentState != State.Disconnected)
                throw new Exception("You can only connect to a device after you have scanned for devices. Maybe it's not " +
                                    "true though :)");

            SetState(State.Connecting);
            Observable
                .Timer(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ =>
                {
                    BluetoothLEHardwareInterface.ConnectToPeripheral(device.Address, null, null,
                        (address, serviceUuid, characteristicUuid) =>
                        {
                            SetState(State.Connected);
                            onCharacteristicFound?.Invoke(new Characteristic
                            {
                                Device = device,
                                ServiceUuid = serviceUuid,
                                CharacteristicUuid = characteristicUuid
                            });
                        },
                        disconnectedAddress =>
                        {
                            SetState(State.Disconnected);
                            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                            onDisconnect?.Invoke(disconnectedAddress);
                        });
                });
        }

        /// <summary>
        /// Subscribe to a given characteristic and sets a delegate where the data will be returned whenever the server
        /// device notifies any new data
        /// </summary>
        public static void Subscribe(Characteristic characteristic, OnDataReceived onDataReceived)
        {
            CheckIfInitialized();

            if (CurrentState != State.Connected && CurrentState != State.Subscribing)
                throw new Exception("You need to be connected to a device before you can subscribe to any characteristic");

            SetState(State.Subscribing);
            Observable
                .Timer(TimeSpan.FromSeconds(2f))
                .Subscribe(_ =>
                {
                    Debug.Log("Subscribing..");
                    BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(characteristic.Device.Address,
                        characteristic.ServiceUuid, characteristic.CharacteristicUuid, 
                        (__, ___) => SetState(State.Communicating),
                        (address, characteristicUuid, bytes) =>
                        {
                            onDataReceived?.Invoke(new Data
                            {
                                Characteristic = characteristic,
                                RawData = bytes
                            });
                        });
                });
        }

        /// <summary>
        /// Unregister from a given characteristic
        /// </summary>
        public static void Unsubscribe(Characteristic characteristic)
        {
            CheckIfInitialized();

            BluetoothLEHardwareInterface.UnSubscribeCharacteristic (characteristic.Device.Address, 
                characteristic.ServiceUuid, characteristic.CharacteristicUuid, null);
        }

        /// <summary>
        /// Disconnects the whole BLE Service.
        /// </summary>
        /// <param name="device"></param>
        public static void Disconnect(Device device)
        {
            CheckIfInitialized();

            if (CurrentState < State.Connected)
                throw new Exception("You need to be connected to be able to disconnect");

            SetState(State.Disconnecting);
            Observable
                .Timer(TimeSpan.FromSeconds(4f))
                .Subscribe(_ =>
                {
                    BluetoothLEHardwareInterface.DisconnectPeripheral (device.Address, address => {
                        DeInitialize();
                        SetState(State.ReadyToScan);
                        IsInitialized = false;
                    });
                });
        
        }

        /// <summary>
        /// DeInitializes the BLE Service, so you can start once again. This will be called once application quits.
        /// </summary>
        private static void DeInitialize()
        {
            CheckIfInitialized();

            BluetoothLEHardwareInterface.DeInitialize(() =>
            {
                IsInitialized = false;
            });
        }

        private static void CheckIfInitialized()
        {
            if (!IsInitialized)
                throw new Exception("You need to initialize first before you can this action.");
        }

        private static void SetState (State newState)
        {
            CurrentState = newState;
        }

        public static void WriteToCharacteristic(Characteristic characteristic, byte[] data,
            bool withResponse, Action<string> action)
        {
            CheckIfInitialized();

            BluetoothLEHardwareInterface.WriteCharacteristic(characteristic.Device.Address, characteristic.ServiceUuid,
                characteristic.CharacteristicUuid, data, data.Length, withResponse, action);
        }

        public static void ReadCharacteristic(Characteristic characteristic, Action<string, byte[]> result)
        {
            CheckIfInitialized();

            BluetoothLEHardwareInterface.ReadCharacteristic(characteristic.Device.Address, characteristic.ServiceUuid,
                characteristic.CharacteristicUuid, result);
        }
    }
}
