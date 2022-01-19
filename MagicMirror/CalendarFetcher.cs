using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicMirror.Pages
{
    public class CalendarFetcher
    {
        public const string TODAY = "Today";
        public const string TOMORROW = "Tomorrow";

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        public Calendar_Wrapper Fetch(bool isTestMode, bool forceUpdate)
        {
            Calendar_Wrapper? result = null;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"cache", "calendar.json");
            try
            {
                // only update every hour, otherwise use cached file
                // except between 7am and 9am, then update every minute!
                var theNoo = DateTime.Now;
                if (isTestMode || (theNoo.Minute != 0))
                {
                    if (!forceUpdate && File.Exists(filePath))
                    {

                        var calJson = File.ReadAllText(filePath);
                        result = JsonConvert.DeserializeObject<Calendar_Wrapper>(calJson); 
                    }
                }
                
                if(result != null)
                {
                    return result;
                }
                result = new Calendar_Wrapper();

                UserCredential credential;

                // TODO : Make this use a service account, at the minute I have to manually refresh the token
                // every seven days!
                using (var stream =
                    new FileStream("credentials_desktop.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Calendar API service.
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define parameters of request.
                EventsResource.ListRequest request = service.Events.List(Constants.Calendar_Id);
                request.TimeMin = DateTime.Now;
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 10;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                // List events.
                Events events = request.Execute();
                // Console.WriteLine("Upcoming events:");
                if (events.Items != null && events.Items.Count > 0)
                {
                    foreach (var eventItem in events.Items)
                    {
                        string when = "All Day";
                        if (eventItem.Start.DateTime?.TimeOfDay != null)
                        {
                            when = eventItem.Start.DateTime.Value.ToString("HH:mm");
                        }

                        string today_tomorrow = String.Empty;
                        var today_str = DateTime.Today.ToString("yyyy-MM-dd");
                        var tomorrow = DateTime.Today.AddDays(1);
                        var tomorrow_str = DateTime.Today.ToString("yyyy-MM-dd");
                        if (eventItem.Start.DateTime?.Date == DateTime.Today || eventItem.Start.Date == today_str)
                        {
                            today_tomorrow = TODAY;
                        }
                        else
                        if (eventItem.Start.DateTime?.Date == tomorrow || eventItem.Start.Date == tomorrow_str)
                        {
                            today_tomorrow = TOMORROW;
                        }
                        
                        if (string.IsNullOrEmpty(today_tomorrow))
                        {
                            continue;
                        }

                        result.Events.Add(
                           new Calendar_Event
                           {
                               Title = eventItem.Summary,
                               Day = today_tomorrow,
                               Time = when
                           });
                    }
                }
                else
                {
                    // Console.WriteLine("No upcoming events found.");
                }
            }
            catch (Exception ex)
            {
              // result.Events.Add(new Calendar_Event { Title = ex.ToString(), Day = TODAY });
            }

            if(result != null)
            {
                var j = JsonConvert.SerializeObject(result);
                File.WriteAllText(filePath, j);
            }

            return result;
        }
    }
}