using System;
using Yuki.Core.Interfaces.User;

namespace Yuki.Core.Implementations.User
{
    class User : IUser
    {
        private string name;
        public string Name { get => name; set => name = value; }

        private static string staticPropertyValueNotAccessibleViaInterface = "Static information";
        public static string StaticPropertyValueNotAccessibleViaInterface =>
            staticPropertyValueNotAccessibleViaInterface;

        public void Init()
        {
            name = "No name set yet";
        }

        public string SayHello()
        {
            return String.Format("Hi!, I'm {0}", Name);
        }
    }
}
