namespace BLEServices
{
    public static partial class BLEService
    {
        public struct Data
        {
            public Characteristic Characteristic { get; internal set; }
            public byte[] RawData;
        }
    }
}