namespace MagicMirror
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using System.Net.Http.Headers;

    public class Weather_Fetcher
    {
        public Weather_Wrapper Fetch(bool isTestMode, bool forceUpdate, bool isDaily)
        {
            Weather_Wrapper? result = null;

            // only update every ten mins, otherwise use cached file
            // except between 7am and 9am, then update every minute!
            string weatherJson = string.Empty;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"cache", isDaily ? "weather_daily.json" : "weather_threely.json");
            var theNoo = DateTime.Now;
            if (isTestMode || (theNoo.Minute % 30 != 0))
            {
                if (!forceUpdate && File.Exists(filePath))
                {
                    weatherJson = File.ReadAllText(filePath);
                }
            }

            try
            {
                // if no cached bus json then fetch
                if (string.IsNullOrWhiteSpace(weatherJson))
                {
                    var url = $"https://rgw.5878-e94b1c46.eu-gb.apiconnect.appdomain.cloud/metoffice/production/v0/forecasts/point/three-hourly";
                    if (isDaily) 
                    {
                        url = $"https://rgw.5878-e94b1c46.eu-gb.apiconnect.appdomain.cloud/metoffice/production/v0/forecasts/point/daily";
                    }
                    //var url = $"https://rgw.5878-e94b1c46.eu-gb.apiconnect.appdomain.cloud/metoffice/production/v0/forecasts/point/hourly";
                    var parameters = $"?latitude={Constants.Weather_Lat}&longitude={Constants.Weather_Lng}";

                    HttpClient client = new HttpClient();
                    var httpRequestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(url + parameters),
                        Headers = {
                            { "X-IBM-Client-Id", Constants.Weather_Client_Id },
                            { "X-IBM-Client-Secret", Constants.Weather_Client_Secret }
                        }
                    };

                    HttpResponseMessage response = client.SendAsync(httpRequestMessage).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        weatherJson = response.Content.ReadAsStringAsync().Result;
                        File.WriteAllText(filePath, weatherJson);
                    }
                }

                result = JsonConvert.DeserializeObject<Weather_Wrapper>(weatherJson);
            }
            catch (Exception ex)
            {
                var foo = ex.ToString();
            }

            return result;
        }
     }
}
