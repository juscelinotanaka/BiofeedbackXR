namespace BLEServices
{
    public static partial class BLEService
    {
        public enum State
        {
            ReadyToScan,
            Disconnecting,
            Disconnected,
            Scanning,
            Connecting,
            Connected,
            Subscribing,
            Communicating,
            Unknown
        }
    }
}