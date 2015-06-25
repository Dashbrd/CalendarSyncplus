namespace CalendarSyncPlus.Domain.Models.Preferences
{
    public class OutlookSettings
    {
        public OutlookOptionsEnum OutlookOptions { get; set; }
        public string OutlookProfileName { get; set; }
        public OutlookMailBox OutlookMailBox { get; set; }
        public OutlookFolder OutlookCalendar { get; set; }
    }
}