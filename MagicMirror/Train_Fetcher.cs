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

    public class Train_Fetcher
    {
        public Train_Wrapper? Fetch(bool isTestMode, bool forceUpdate)
        {
            Train_Wrapper? result = null;

            // only update every ten mins, otherwise use cached file
            // except between 7am and 9am, then update every minute!
            string trainJson = string.Empty;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"cache", "trains.json");
            var theNoo = DateTime.Now;
            if (isTestMode || (theNoo.Minute % 10 != 0 && (theNoo.Hour < 7 || theNoo.Hour > 8)))
            {
                if (!forceUpdate && File.Exists(filePath))
                {
                    trainJson = File.ReadAllText(filePath);
                }
            }

            try
            {
                // if no cached bus json then fetch
                if (string.IsNullOrWhiteSpace(trainJson))
                {
                    var url = $"https://transportapi.com/v3/uk/train/station/{Constants.Train_Station_Code}/live.json";
                    var parameters = $"?app_id={Constants.Train_App_Id}&app_key={Constants.Train_Api_Secret}&darwin=false&train_status=passenger";

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
                        trainJson = response.Content.ReadAsStringAsync().Result;
                        File.WriteAllText(filePath, trainJson);
                    }
                }

                result = JsonConvert.DeserializeObject<Train_Wrapper>(trainJson);

                // we're not interested in certain destinations so filter those out
                //if (result?.departures?.All != null)
                //{
                //    result.departures.All.RemoveAll(x => x.destination_name == "Venus");
                //    result.departures.All.RemoveAll(x => x.destination_name == "Mars");
                //}

            }
            catch (Exception ex)
            {
                var foo = ex.ToString();
            }

            return result;
        }

    }
}
