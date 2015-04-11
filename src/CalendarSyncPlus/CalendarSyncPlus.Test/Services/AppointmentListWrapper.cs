using System.Collections.Generic;
using Test.Model;

namespace Test.Services
{
    public class AppointmentListWrapper
    {
        public List<Appointment> Appointments { get; set; }

        public bool WaitForApplicationQuit { get; set; }
    }
}