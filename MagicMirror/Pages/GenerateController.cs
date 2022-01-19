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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MagicMirror.Pages
{
    [ApiController]
    [Route("api/generate")]
    public class GenerateController : ControllerBase
    {

        private bool _is_test_mode = false;
        private const int _max_buses_trains = 7;

        // GET: api/<ValuesController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var forceqs = $"{Request.Query["force"]}";
                bool forceUpdate = (forceqs == "true");

                // init fonts 
                var text_t = SystemFonts.CreateFont("Calibri", 50);
                var text_h = SystemFonts.CreateFont("Calibri", 32);
                var text_b = SystemFonts.CreateFont("Calibri", 20, FontStyle.Bold);
                var text_n = SystemFonts.CreateFont("Calibri", 20);

                var font_options_centered = new DrawingOptions
                {
                    TextOptions = new TextOptions
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                };

                var font_options_right = new DrawingOptions
                {
                    TextOptions = new TextOptions
                    {
                        HorizontalAlignment = HorizontalAlignment.Right
                    }
                };

                // init variables
                //float x_pos = 10.0f;
                float y_pos = 0.0f;

                float y_t_spacing = 60;
                //float y_h_spacing = 36;
                float y_n_spacing = 25;

                int counter = 0;

                var pen_thin = new Pen(Color.Grey, 1);
                var image = new Image<Rgb24>(600, 800, Color.White);

                DateTime theNoo = DateTime.Now;

                //
                // time - top
                //
                var the_time = DateTime.Now;
                string str_time = String.Empty;
                if(the_time.Second >= 30)
                {
                    // round to nearest minute, otherwise the display could (in theory) be seen to
                    // be running up to 59 seconds slow!
                    str_time = the_time.AddMinutes(1).ToString("HH:mm");
                }
                else
                {
                    str_time = the_time.ToString("HH:mm");
                }
                image.Mutate(x => x.DrawText(str_time, text_t, Color.Black, new PointF(242, 20)));

                // day on left and date on right
                image.Mutate(x => x.DrawText(font_options_right, theNoo.ToString("d MMM"), text_b, Color.Grey, new PointF(570, 20)));
                image.Mutate(x => x.DrawText(theNoo.ToString("dddd"), text_b, Color.Grey, new PointF(30, 20)));
                y_pos = 20 + y_t_spacing;



                //
                // BUS + TRAIN HEADINGS
                //
                image.Mutate(x => x.DrawText(font_options_right, "BUSES", text_b, Color.Black, new PointF(570, y_pos)));
                image.Mutate(x => x.DrawText("TRAINS", text_b, Color.Black, new PointF(30, y_pos)));
                y_pos += y_n_spacing + 10;

                //
                // BUS TIMES
                //
                var bus_fetcher = new Bus_Fetcher();
                var bus_times = bus_fetcher.Fetch(_is_test_mode, forceUpdate);
                if (bus_times?.Departures?.Departures != null)
                {
                    float y_bus = y_pos;
                    image.Mutate(x => x.DrawText(font_options_right, $"Time", text_b, Color.Grey, new PointF(570, y_bus)));
                    y_bus += y_n_spacing;

                    counter = 0;
                    foreach (var bus_time in bus_times.Departures.Departures)
                    {
                        image.Mutate(x => x.DrawText(font_options_right, $"{bus_time.GetTime()}", text_n, Color.Black, new PointF(570, y_bus)));
                        y_bus += y_n_spacing;

                        // max to show
                        counter++;
                        if (counter == _max_buses_trains)
                        {
                            break;
                        }
                    }
                }

                //
                // TRAIN TIMES
                //
                var train_fetcher = new Train_Fetcher();
                var train_times = train_fetcher.Fetch(_is_test_mode, forceUpdate);
                if (train_times?.departures?.All != null)
                {
                    int x_col_aimed = 30;
                    int x_col_expected = 100;
                    int x_col_platform = 170;
                    int x_col_destination = 200;

                    float y_train = y_pos;

                    image.Mutate(x => x.DrawText($"Time", text_b, Color.Grey, new PointF(x_col_aimed, y_train)));
                    image.Mutate(x => x.DrawText($"Exp.", text_b, Color.Grey, new PointF(x_col_expected, y_train)));
                    image.Mutate(x => x.DrawText(font_options_centered, $"P.", text_b, Color.Grey, new PointF(x_col_platform + 5, y_train)));
                    image.Mutate(x => x.DrawText($"Dest", text_b, Color.Grey, new PointF(x_col_destination, y_train)));

                    y_train += y_n_spacing;

                    counter = 0;
                    foreach (var train_time in train_times.departures.All)
                    {

                        image.Mutate(x => x.DrawText($"{train_time.aimed_departure_time}", text_n, Color.Black, new PointF(x_col_aimed, y_train)));
                        image.Mutate(x => x.DrawText($"{train_time.GetExpected()}", text_n, Color.Black, new PointF(x_col_expected, y_train)));
                        image.Mutate(x => x.DrawText(font_options_centered, $"{train_time.GetPlatform()}", text_n, Color.Grey, new PointF(x_col_platform + 5, y_train)));
                        image.Mutate(x => x.DrawText($"{train_time.destination_name}", text_n, Color.Black, new PointF(x_col_destination, y_train)));


                        y_train += y_n_spacing;

                        // max to show
                        counter++;
                        if (counter == _max_buses_trains)
                        {
                            break;
                        }
                    }
                }


                // dividers
                y_pos = y_pos + y_n_spacing + (y_n_spacing * _max_buses_trains) - 8;
                DrawLine(image, 480, 82, 480, y_pos, pen_thin);

                y_pos += 20;
                DrawLine(image, 30, y_pos, 570, y_pos, pen_thin);


                //
                // WEATHER
                //
                var wf = new Weather_Fetcher();
                var weather_daily = wf.Fetch(_is_test_mode, forceUpdate, true);
                var weather_threely = wf.Fetch(_is_test_mode, forceUpdate, false);

                // we want the first four from threely and then "tomorrow" and "overmorrow"
                var threely_events = weather_threely?.features?.FirstOrDefault()?.properties?.timeSeries;
                var daily_events = weather_daily?.features?.FirstOrDefault()?.properties?.timeSeries;

                var weather_events = new List<Weather_Event>();

                if (threely_events != null && threely_events.Any())
                {
                    var minTime = DateTime.Now.AddHours(-3); // so that at 2:59pm it still shows the 12->3 weather etc.
                    weather_events.AddRange(threely_events.Where(x => x.time > minTime).Take(4));
                }

                if (daily_events != null && daily_events.Any())
                {
                    var minTime = DateTime.Now.Date.AddDays(1); 
                    weather_events.AddRange(daily_events.Where(x => x.time >= minTime).Take(2));
                }

                int x_weather = 30;
                int y_weather = 345;
                int weather_limit = 6;

                image.Mutate(x => x.DrawText("WEATHER", text_b, Color.Black, new PointF(x_weather, y_weather)));

                if (weather_events != null)
                {
                    // display wind warning?
                    if (weather_events.Any(x => x.max10mWindGust > 25))
                    {
                        image.Mutate(x => x.DrawText(font_options_right, "WARNING : WIND > 25mph", text_b, Color.Black, new PointF(570, y_weather)));
                    }

                    y_weather = 435;
                    y_weather += (int)y_n_spacing;
                    image.Mutate(x => x.DrawText($"T", text_n, Color.Grey, new PointF(x_weather, y_weather)));
                    y_weather += (int)y_n_spacing;
                    image.Mutate(x => x.DrawText($"R", text_n, Color.Grey, new PointF(x_weather, y_weather)));
                    y_weather += (int)y_n_spacing;
                    image.Mutate(x => x.DrawText($"W", text_n, Color.Grey, new PointF(x_weather, y_weather)));
                    x_weather = 92;

                    counter = 0;
                    foreach (var weather_event in weather_events)
                    {
                        y_weather = 440;

                        var weather_icon = GetWeatherIcon(weather_event.Code);
                        if (weather_icon != null) 
                        {
                            image.Mutate(x => x.DrawImage(weather_icon, new Point(x_weather - 24, y_weather - 60), 1f));
                        }

                        image.Mutate(x => x.DrawText(font_options_centered, $"{weather_event.TimeStamp}", text_n, Color.Black, new PointF(x_weather, y_weather)));
                        y_weather += (int)y_n_spacing;

                        image.Mutate(x => x.DrawText(font_options_centered, $"{weather_event.Temp}°", text_n, Color.Black, new PointF(x_weather, y_weather)));
                        y_weather += (int)y_n_spacing;

                        image.Mutate(x => x.DrawText(font_options_centered, $"{weather_event.Rain}%", text_n, Color.Grey, new PointF(x_weather, y_weather)));
                        y_weather += (int)y_n_spacing;

                        image.Mutate(x => x.DrawText(font_options_centered, $"{weather_event.Wind:0}", text_n, Color.Grey, new PointF(x_weather, y_weather)));
                        y_weather += (int)y_n_spacing;

                        counter++;
                        x_weather += 90;
                        if(counter == weather_limit)
                        {
                            break;
                        }
                    }
                }

                y_pos = 550;
                DrawLine(image, 30, y_pos, 570, y_pos, pen_thin);

                //
                // Calendar events
                //
                var cf = new CalendarFetcher();
                var event_wrapper = cf.Fetch(_is_test_mode, forceUpdate);

                if (event_wrapper != null)
                {
                    var events = event_wrapper.Events;
                    int x_col_event_time = 30;
                    int x_col_event_text = 100;
                    int max_events_today_only = 6;
                    int max_events_inc_tomorrow = max_events_today_only - 2;        // "Tomorrow" heading takes up 2 spaces
                    counter = 0;

                    y_pos = 570;

                    image.Mutate(x => x.DrawText("CALENDAR", text_b, Color.Black, new PointF(x_col_event_time, y_pos)));
                    y_pos += y_n_spacing + 10;

                    var filteredEvents = events.Where(x => x.Day == CalendarFetcher.TODAY);
                    if (filteredEvents.Any())
                    {
                        image.Mutate(x => x.DrawText(CalendarFetcher.TODAY, text_b, Color.Grey, new PointF(x_col_event_time, y_pos)));
                        y_pos += y_n_spacing;

                        foreach (var calendarEvent in filteredEvents)
                        {
                            image.Mutate(x => x.DrawText(calendarEvent.Time, text_n, Color.Black, new PointF(x_col_event_time, y_pos)));
                            image.Mutate(x => x.DrawText(calendarEvent.Title, text_n, Color.Black, new PointF(x_col_event_text, y_pos)));
                            y_pos += y_n_spacing;
                            counter++;

                            if(counter >= max_events_today_only)
                            {
                                break;
                            }
                        }
                    }

                    // don't show "tomorrow" if there isn't space
                    if (counter < max_events_inc_tomorrow)
                    {
                        filteredEvents = events.Where(x => x.Day == CalendarFetcher.TOMORROW);
                        if (filteredEvents.Any())
                        {
                            if (counter > 0)
                            {
                                y_pos += y_n_spacing;
                            }
                            image.Mutate(x => x.DrawText(CalendarFetcher.TOMORROW, text_b, Color.Grey, new PointF(x_col_event_time, y_pos)));
                            y_pos += y_n_spacing;

                            foreach (var calendarEvent in filteredEvents)
                            {
                                image.Mutate(x => x.DrawText(calendarEvent.Time, text_n, Color.Black, new PointF(x_col_event_time, y_pos)));
                                image.Mutate(x => x.DrawText(calendarEvent.Title, text_n, Color.Black, new PointF(x_col_event_text, y_pos)));
                                y_pos += y_n_spacing;
                                counter++;

                                if (counter >= max_events_inc_tomorrow)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }



                //
                // serve as PNG
                //
                using (var memoryStream = new MemoryStream())
                {
                    var imageEncoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);
                    ((PngEncoder)imageEncoder).BitDepth = PngBitDepth.Bit8;
                    ((PngEncoder)imageEncoder).ColorType = PngColorType.Grayscale;
                    image.Save(memoryStream, imageEncoder);
                    return File(memoryStream.ToArray(), "image/png");
                }
            }
            catch(Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

        private void DrawLine(Image image, float x1, float y1, float x2, float y2, Pen linePen)
        {
            var points = new PointF[2];
            points[0] = new PointF(
                x: x1,
                y: y1);
            points[1] = new PointF(
                x: x2,
                y: y2);
            image.Mutate(x => x
                .DrawLines(linePen, points)
            );
        }


        private Dictionary<int, Image<Rgba32>>? _icon_cache;

        private Image<Rgba32>? GetWeatherIcon(int id)
        {
            if(_icon_cache == null)
            {
                _icon_cache = new Dictionary<int, Image<Rgba32>>();
            }

            if(_icon_cache.ContainsKey(id))
            {
                return _icon_cache[id];
            }
            try
            {
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), @"w", $"{id}.png");
                if (System.IO.File.Exists(imgPath))
                {
                    Image<Rgba32> img2 = Image.Load<Rgba32>(imgPath);
                    _icon_cache.Add(id, img2);
                    return img2;
                }
            }
            catch(Exception ex)
            {

            }
            return null;
        }
       



    }
}
