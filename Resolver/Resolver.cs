using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Yuki.Core.Interfaces;
using Yuki.Core.Interfaces.Vat;
using Yuki.Core.Resolver.Countries;
using Yuki.Core.Resolver.Infrastructure;

namespace Yuki.Core.Resolver
{
    public class Resolver : Singleton<Resolver>
    {

        private IDictionary<string, IDictionary<Country, string>> mappings;
        private Assembly implementationsAssembly;

        private Resolver()
        {

            mappings = new Dictionary<string, IDictionary<Country, string>>();
            implementationsAssembly =
             Assembly.LoadFile(string.Format("{0}..\\..\\..\\Yuki.Core.Implementations\\obj" +
             "\\Debug\\Yuki.Core.Implementations.dll", AppContext.BaseDirectory));

            DoMappings();

        }

        private static string errMsgNotImplemented = "{0} not implemented.";
        private static Resolver InitializeInstance()
        {
            return new Resolver();
        }

        private void DoMappings()
        {  //This could be done using a xml configuration file.
           //We could map all the type but only goona map those having multiple implementations
           //for single matching (interface => 1 implementation) we resolve using Linq
            var VatImplementationsDic = new Dictionary<Country, string>
            {
                { Country.Belgium, "VATBelgium" },
                { Country.Netherlands, "VATNetherlands" },
                { Country.Spain, "VATSpain" }
            };
            mappings.Add(typeof(IVat).Name, VatImplementationsDic);
        }

        public static T Resolve<T>() where T : IDataComponent
        {
            return GetInstance().GetImplementation<T>();
        }

        public static T Resolve<T>(Country country) where T : IDataComponent
        {
            try
            {
                string interfaceName = typeof(T).Name;
                string implementationName = GetInstance().mappings[interfaceName][country];
                return GetInstance().GetImplementation<T>(implementationName);
            }
            catch (Exception e)
            {
                //Handle exception, out of scoope now.
                throw e;
            }
        }

        private T GetImplementation<T>() where T : IDataComponent
        {
            Type interfaceArgType = typeof(T);

            Type implementationType = (from type in implementationsAssembly.GetTypes()
                                       where interfaceArgType.IsAssignableFrom(type)
                                       && type.IsInterface == false
                                       select type).FirstOrDefault();

            if (implementationType == null || typeof(T) == typeof(IDataComponent))
            {
                throw new NotImplementedException(string.Format(errMsgNotImplemented,
                        typeof(T).ToString()));
            }

            var instance = implementationsAssembly.CreateInstance(implementationType.FullName);

            if (instance == null)
            {
                List<Type> types = new List<Type>
                {
                    interfaceArgType
                };
                throw new ReflectionTypeLoadException(types.ToArray(), null, "Unable to load instance");
            }

            return (T)instance;
        }

        private T GetImplementation<T>(string implementationTypeName) where T : IDataComponent
        {
            Type interfaceType = typeof(T);

            Type implementationType = (from type in implementationsAssembly.GetTypes()
                                       where interfaceType.IsAssignableFrom(type)
                                       && type.IsInterface == false
                                       && type.Name.ToLower() == implementationTypeName.ToLower()
                                       select type).FirstOrDefault();

            if (implementationType == null || typeof(T) == typeof(IDataComponent))
            {
                throw new NotImplementedException(string.Format(errMsgNotImplemented,
                        typeof(T).ToString()));
            }

            var instance = implementationsAssembly.CreateInstance(implementationType.FullName);

            if (instance == null)
            {
                List<Type> types = new List<Type>
                {
                    interfaceType
                };
                throw new ReflectionTypeLoadException(types.ToArray(), null, "Unable to load instance");
            }

            return (T)instance;
        }
    }
}