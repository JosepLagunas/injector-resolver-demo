using System;
using System.Collections.Generic;
using Yuki.Core.Interfaces.Calendar;
using Yuki.Core.Interfaces.User;
using Yuki.Core.Resolver;

namespace Yuki.Core.Implementations.Calendar
{
    public class Calendar : ICalendar
    {
        private DateTime start;
        public DateTime Start { get => start; set => start = value; }
        private double duration;
        public double Duration { get => duration; set => duration = value; }

        private IList<IUser> attendees;
        public IList<IUser> Attendees { get => attendees; }

        private DateTime initiatedAt;
        public DateTime InitiatedAt => initiatedAt;

        public void AddAttendee(string name)
        {
            IUser user = Yuki.Core.Resolver.Resolver.Resolve<IUser>();
            user.Init();
            user.Name = name;
            attendees.Add(user);
        }


        public string GetAppointmentInfo()
        {
            return string.Format("Start: {0}, Duration: {1}, Number of attendees: {2}",
                start, duration, attendees.Count);
        }

        public void Init()
        {
            attendees = new List<IUser>();
            initiatedAt = DateTime.Now;
        }
    }
}
