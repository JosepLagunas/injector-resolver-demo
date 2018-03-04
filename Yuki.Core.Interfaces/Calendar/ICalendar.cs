using System;
using System.Collections.Generic;
using System.Text;
using Yuki.Core.Interfaces.User;

namespace Yuki.Core.Interfaces.Calendar
{
    public interface ICalendar : IDataComponent
    {
        DateTime InitiatedAt { get; }
        DateTime Start { get; set; }
        double Duration { get; set; }
        String GetAppointmentInfo();
        IList<IUser> Attendees { get; }
        void AddAttendee(string name);
    }
}
