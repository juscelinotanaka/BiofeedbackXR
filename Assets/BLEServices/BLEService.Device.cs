namespace BLEServices
{
    public static partial class BLEService
    {
        public struct Device
        {
            public string Name { get; internal set; }
            public string Address { get; internal set; }
        }
    }
}