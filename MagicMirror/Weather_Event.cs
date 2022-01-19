namespace MagicMirror
{
    public class Weather_Event
    {
        // shared
        public DateTime time { get; set; }

        // daily
        public decimal dayMaxScreenTemperature { get; set; }
        //public decimal nightMinScreenTemperature { get; set; }
        public int daySignificantWeatherCode { get; set; }
        public decimal dayProbabilityOfPrecipitation { get; set; }
        public decimal midday10MWindGust { get; set; }

        //midday10MWindGust
        //public int nightSignificantWeatherCode { get; set; }
        // public int maxUvIndex { get; set; }

        // three-hourly

        public int significantWeatherCode { get; set; }
        public decimal maxScreenAirTemp { get; set; }
        public decimal minScreenAirTemp { get; set; }
        public decimal probOfPrecipitation { get; set; }
        public decimal windSpeed10m { get; set; }
        public decimal max10mWindGust { get; set; }

        public int Code
        {
            get
            {
                if (significantWeatherCode != 0)
                {
                    return significantWeatherCode;
                }
                return Convert.ToInt32(daySignificantWeatherCode);
            }
        }

        public string TimeStamp
        {
            get
            {
                if (maxScreenAirTemp != 0 || probOfPrecipitation != 0)
                {
                    return this.time.ToString("H:mm");
                }

                var num = this.time.Day;
                switch (num % 100)
                {
                    case 11:
                    case 12:
                    case 13:
                        return num + "th";
                }

                switch (num % 10)
                {
                    case 1:
                        return num + "st";
                    case 2:
                        return num + "nd";
                    case 3:
                        return num + "rd";
                    default:
                        return num + "th";
                }
            }
        }


        public int Temp
        {
            get
            {
                if (maxScreenAirTemp != 0)
                {
                    return Convert.ToInt32(maxScreenAirTemp);
                }
                return Convert.ToInt32(dayMaxScreenTemperature);
            }
        }

        public int Rain
        {
            get
            {
                if (probOfPrecipitation != 0)
                {
                    return Convert.ToInt32(probOfPrecipitation);
                }
                return Convert.ToInt32(dayProbabilityOfPrecipitation);
            }
        }

        public int Wind
        {
            get
            {
                if (max10mWindGust != 0)
                {
                    return Convert.ToInt32(max10mWindGust);
                }
                return Convert.ToInt32(midday10MWindGust);
            }
        }
    }
}
