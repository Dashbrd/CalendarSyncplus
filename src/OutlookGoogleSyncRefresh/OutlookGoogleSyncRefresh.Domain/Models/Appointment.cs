using System;
using OutlookGoogleSyncRefresh.Domain.Helpers;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class Appointment : Model
    {
        private bool _allDayEvent;
        private string _appointmentId;
        private string _description;
        private DateTime? _endTime;
        private string _location;
        private string _optionalAttendees;
        private string _organizer;
        private int _reminderMinutesBeforeStart;
        private bool _reminderSet;
        private string _requiredAttendees;
        private DateTime? _startTime;
        private string _subject;
        private string _visibility;
        private string _transparency;

        public Appointment(string description, string location, string subject, DateTime? endTime, DateTime? startTime,
            string appointmentId)
            : this(description, location, subject, endTime, startTime)
        {
            _appointmentId = appointmentId;
        }

        public Appointment(string description, string location, string subject, DateTime? endTime, DateTime? startTime)
        {
            _description = description;
            _location = location;
            _subject = subject;
            _endTime = endTime;
            _startTime = startTime;
        }

        public DateTime? StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        public DateTime? EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value); }
        }

        public string Subject
        {
            get { return _subject; }
            set { SetProperty(ref _subject, value); }
        }

        public string Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public string AppointmentId
        {
            get { return _appointmentId; }
            set { SetProperty(ref _appointmentId, value); }
        }

        public string Rfc339FormatStartTime
        {
            get { return StartTime.Value.Rfc339FFormat(); }
        }


        public string Rfc339FormatEndTime
        {
            get { return EndTime.Value.Rfc339FFormat(); }
        }

        public bool AllDayEvent
        {
            get { return _allDayEvent; }
            set { SetProperty(ref _allDayEvent, value); }
        }

        public bool ReminderSet
        {
            get { return _reminderSet; }
            set { SetProperty(ref _reminderSet, value); }
        }

        public int ReminderMinutesBeforeStart
        {
            get { return _reminderMinutesBeforeStart; }
            set { SetProperty(ref _reminderMinutesBeforeStart, value); }
        }

        public string Organizer
        {
            get { return _organizer; }
            set { SetProperty(ref _organizer, value); }
        }

        public string RequiredAttendees
        {
            get { return _requiredAttendees; }
            set { SetProperty(ref _requiredAttendees, value); }
        }

        public string OptionalAttendees
        {
            get { return _optionalAttendees; }
            set { SetProperty(ref _optionalAttendees, value); }
        }

        public string Visibility
        {
            get { return _visibility; }
            set { SetProperty(ref _visibility, value); }
        }

        public string Transparency
        {
            get { return _transparency; }
            set { SetProperty(ref _transparency, value); }
        }

        public override bool Equals(Object obj)
        {
            // Check if the object is a Appointment.
            // The initial null check is unnecessary as the cast will result in null
            // if obj is null to start with.
            var appointment = obj as Appointment;

            if (appointment == null)
            {
                // If it is null then it is not equal to this instance.
                return false;
            }
            // Instances are considered equal if the ToString matches.
            return ToString() == appointment.ToString();
        }

        public override int GetHashCode()
        {
            // Returning the hashcode of the Guid used for the ToString() will be 
            // sufficient and would only cause a problem if Appointments objects
            // were stored in a non-generic hash set along side other guid instances
            // which is very unlikely!
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return Rfc339FormatStartTime + ";" + Rfc339FormatStartTime + ";" + Subject + ";" + Location;
        }
    }
}