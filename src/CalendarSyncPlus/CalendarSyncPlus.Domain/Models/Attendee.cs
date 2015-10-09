namespace CalendarSyncPlus.Domain.Models
{
    public class Attendee
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public MeetingResponseStatusEnum MeetingResponseStatus { get; set; }
    }
}