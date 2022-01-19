namespace MagicMirror
{
    public class Bus_Departure
    {
        public Bus_ArrivalDeparture? Aimed { get; set; }
        public Bus_ArrivalDeparture? Expected { get; set; }

        public string GetTime()
        {
            return this.Expected?.Departure?.Time ?? this.Aimed?.Departure?.Time ?? "";
        }
    }
}
