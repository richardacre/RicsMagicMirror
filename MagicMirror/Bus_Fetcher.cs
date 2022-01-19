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

    public class Bus_Fetcher
    {
        private const int _max_buses_trains = 7;

        // 
        public Bus_Wrapper? Fetch(bool isTestMode, bool forceUpdate)
        {
            Bus_Wrapper? result = null;

            // only update every ten mins, otherwise use cached file
            // except between 7am and 9am, then update every minute!
            string busJson = string.Empty;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"cache", "buses.json");
            var theNoo = DateTime.Now;
            if (isTestMode || (theNoo.Minute % 10 != 0 && (theNoo.Hour < 7 || theNoo.Hour > 8)))
            {
                if (!forceUpdate && File.Exists(filePath))
                {
                    busJson = File.ReadAllText(filePath);
                }
            }

            try
            {
                // if no cached bus json then fetch
                if (string.IsNullOrWhiteSpace(busJson))
                {
                    var url = $"https://transportapi.com/v3/uk/bus/stop/{Constants.Bus_Stop_Id}/live.json";
                    var parameters = $"?app_id={Constants.Bus_App_Id}&app_key={Constants.Bus_Api_Secret}&limit={_max_buses_trains}&group=route&nextbuses=no";

                    HttpClient client = new HttpClient();
                    var httpRequestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(url + parameters),
                        Headers = {
                            { System.Net.HttpRequestHeader.Accept.ToString(), "application/json" }
                        }
                    };

                    HttpResponseMessage response = client.SendAsync(httpRequestMessage).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        busJson = response.Content.ReadAsStringAsync().Result;
                        File.WriteAllText(filePath, busJson);
                    }
                }

                // property for the bus departure is "13" so rename that
                busJson = busJson.Replace("\"" + Constants.Bus_Replace_Number + "\":", "\"departures\":");
                result = JsonConvert.DeserializeObject<Bus_Wrapper>(busJson);
            }
            catch (Exception ex)
            {
                var foo = ex.ToString();
            }

            return result;
        }
    }
}
