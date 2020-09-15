namespace BLEServices
{
    public static partial class BLEService
    {
        public struct Characteristic
        {
            public Device Device { get; internal set; }
            public string ServiceUuid { get; internal set; }
            public string CharacteristicUuid { get; internal set; }
        }
    }
}