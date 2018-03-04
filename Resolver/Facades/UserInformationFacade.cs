using System;
using Yuki.Core.Interfaces;
using Yuki.Core.Interfaces.User;
using Yuki.Core.Resolver;
using Yuki.Core.Resolver.Infrastructure;

namespace Yuki.Common.Facades
{
    public class UserInformationFacade : Singleton<UserInformationFacade>
    {
        public new static UserInformationFacade GetInstance
        {
            get
            {
                return GetInstance();
            }
        }

        private UserInformationFacade() { }

        private static UserInformationFacade InitializeInstance()
        {
            return new UserInformationFacade();
        }

        public string GetStaticPropertyValueNotAccessibleViaInterface()
        {
            object intance = Resolver.Resolve<IUser>();

            return "";

        }
    }
}

