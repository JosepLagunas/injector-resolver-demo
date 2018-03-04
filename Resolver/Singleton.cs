using System;
using System.Linq;
using System.Reflection;

namespace Yuki.Core.Resolver.Infrastructure
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        private static readonly Lazy<T> lazyInstance = new Lazy<T>(InitializeInstance);
        private static object locker = new object();

        private static string errMsgMissingInitMethod =
            "private {0} InitializeInstance() static method not found in {1}, is required.";
        private static string errMsgPublicConstructorsFound =
            "Singleton instance of type {0} cannot contain any public constructor.";

        protected static T GetInstance()
        {
            return lazyInstance.Value;
        }

        private static T InitializeInstance()
        {
            (bool status, Exception exception)
                result = TryToInitializeTypeInstance(out T instanceCreated);

            if (!result.status)
            {
                throw result.exception;
            }

            return instanceCreated;
        }

        private static (bool status, Exception exception)
            TryToInitializeTypeInstance(out T instance)
        {
            instance = default(T);

            Type type = typeof(T);

            if (HasPublicContructors(type))
            {
                MethodAccessException exception = GetPublicConstructorsFoundError(type);
                return (status: false, exception: exception);
            }

            MethodInfo parametrizedTypeInitializator = type.GetMethod("InitializeInstance",
                 BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Static);

            if (parametrizedTypeInitializator == null)
            {
                MissingMethodException exception = GetMissingMethodError(type);
                return (status: false, exception: exception);
            }

            try
            {
                instance = (T)parametrizedTypeInitializator.Invoke(null, new object[] { });
                return (status: true, exception: null);
            }
            catch (Exception e)
            {
                return (status: false, exception: e);
            }
        }

        private static bool HasPublicContructors(Type type)
        {
            return type.GetConstructors().ToList()
                .FirstOrDefault(c => c.IsPublic) != null;
        }

        private static MissingMethodException GetMissingMethodError(Type type)
        {
            return new MissingMethodException(
                string.Format(errMsgMissingInitMethod, type.FullName, type.FullName));
        }

        private static MethodAccessException GetPublicConstructorsFoundError(Type type)
        {
            return
                new MethodAccessException(string.Format(errMsgPublicConstructorsFound, type.FullName));
        }
    }
}

