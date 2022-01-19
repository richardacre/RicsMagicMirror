namespace MagicMirror
{
    public class Train_Departure
    {
        public string? status { get; set; }
        public string? destination_name { get; set; }
        public string? platform { get; set; }
        public string? aimed_departure_time { get; set; }
        public string? expected_departure_time { get; set; }

        public string GetExpected()
        {
            if(!string.IsNullOrWhiteSpace(this.expected_departure_time) && this.expected_departure_time != this.aimed_departure_time)
            {
                return $"({this.expected_departure_time})";
            }

            return string.Empty;
        }

        public string GetPlatform()
        {
            if($"{this.status}".ToLower() == "bus")
            {
                return "Bus";
            }

            return $"{platform}";
        }
    }
}
