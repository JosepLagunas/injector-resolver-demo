using Yuki.Core.Interfaces.Calendar;

namespace Yuki.Core.Interfaces.User
{
   public interface IUser : IDataComponent
    {
        ICalendar Calendar { get; set; }
        string Name { get; set; }
        string SayHello();
    }
}