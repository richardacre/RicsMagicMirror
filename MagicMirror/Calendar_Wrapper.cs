namespace MagicMirror
{
    public class Calendar_Wrapper
    {
        public Calendar_Wrapper()
        {
            this.Events = new List<Calendar_Event>();
        }

        public List<Calendar_Event> Events { get; set; }    
    }
}
