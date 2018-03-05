using System;
using System.Reflection;
using Yuki.Core.Interfaces.User;
using Yuki.Core.Resolver;
using Yuki.Core.Resolver.Infrastructure;

namespace Yuki.Common.Facades
{
    public class UserInformationFacade : Singleton<UserInformationFacade>
    {
        Type implementationType;

        public new static UserInformationFacade GetInstance
        {
            get
            {
                return GetInstance();
            }
        }

        private UserInformationFacade()
        {

            if (!Resolver.GetInstance().TryFindImplementation<IUser>(out Type outputType))
            {
                throw new TypeLoadException("Error loading info from type.");
            }
            implementationType = outputType;
        }

        private static UserInformationFacade InitializeInstance()
        {
            return new UserInformationFacade();
        }

        public string GetStaticPropertyValueNotAccessibleViaInterface()
        {
            try
            {
                return GetPropertyValue<string>("StaticPropertyValueNotAccessibleViaInterface");
            }
            catch (Exception e)
            {
                throw new TypeLoadException("Error accessing static info from type.");
            }
        }

        public int GetStaticMethodValueNotAccessibleViaInterface()
        {
            try
            {
                return GetMethodValue<int>("StaticMethodValueNotAccessibleViaInterface");
            }
            catch (Exception e)
            {
                throw new TypeLoadException("Error accessing static info from type.");
            }
        }

        private T GetPropertyValue<T>(string propertyName)
        {
            return (T)implementationType
                    .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null);
        }

        private T GetMethodValue<T>(string methodName)
        {
            return (T)implementationType
                    .GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null);
        }
    }
}

