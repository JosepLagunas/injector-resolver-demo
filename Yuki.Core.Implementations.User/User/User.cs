using Yuki.Core.Interfaces.Calendar;
using Yuki.Core.Interfaces.User;
using Yuki.Core.Resolver;

namespace Yuki.Core.Implementations.User
{
	public class User : IUser
	{
		private string name;
      public string Name { get => name; set => name = value; }

		public static string StaticPropertyValueNotAccessibleViaInterface { get => "Static info."; }

      ICalendar calendar;
      
      [Inject]
      public ICalendar Calendar { get => calendar; set => calendar = value; }

      public static int StaticMethodValueNotAccessibleViaInterface()
		{
			return 36;
		}

		public void Init()
		{
			Name = "No name set";
		}

		public string SayHello()
		{
			return string.Format("Hi! My name is {0}", Name);
		}
	}
}
