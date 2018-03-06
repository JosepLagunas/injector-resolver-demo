using Yuki.Core.Interfaces.User;

namespace Yuki.Core.Implementations.User
{
	public class User : IUser
	{
		private string name;
		public string Name { get => name; set => name = value; }

		public static string StaticPropertyValueNotAccessibleViaInterface { get => "Static info."; }

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
