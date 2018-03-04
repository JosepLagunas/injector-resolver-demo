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
        private static string loadImplementationError = "Unable to create instance of Type {0}";

        private Resolver()
        {
            InitializeMappingsDictionary();
            LoadImplementationAssembly();
            DoMappings();
        }

        private void InitializeMappingsDictionary()
        {
            mappings = new Dictionary<string, IDictionary<Country, string>>();
        }

        private void LoadImplementationAssembly()
        {
            implementationsAssembly =
                         Assembly.LoadFile(string.Format("{0}..\\..\\..\\Yuki.Core.Implementations\\obj" +
                         "\\Debug\\Yuki.Core.Implementations.dll", AppContext.BaseDirectory));
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
            catch (NotImplementedException e)
            {
                //Handle exception, out of scoope now.
                throw e;
            }
            catch (ReflectionTypeLoadException e)
            {
                //Handle exception, out of scoope now.
                throw e;
            }
            catch(Exception e)
            {
                //Handle exception, out of scoope now.
                throw e;
            }
        }

        private T GetImplementation<T>() where T : IDataComponent
        {
            if (!TryFindImplementation<T>(out Type implementationType))
            {
                ThrowNotImplementedTypeException<T>();
            }

            if (!TryToCreateInstance<T>(implementationType, out T instance))
            {
                ThrowErrorOnTypeLoadException<T>();
            }            

            return (T)instance;
        }
        
        private T GetImplementation<T>(string implementationTypeName) where T : IDataComponent
        {
            implementationTypeName = implementationTypeName.ToLower();

            if (!TryFindImplementation<T>(implementationTypeName, out Type implementationType))
            {
                ThrowNotImplementedTypeException<T>();
            }

            if (!TryToCreateInstance<T>(implementationType, out T instance))
            {
                ThrowErrorOnTypeLoadException<T>();
            }

            return (T)instance;
        }

        private bool TryFindImplementation<T>(out Type implementationType) where T : IDataComponent
        {
            implementationType = FindImplementations<T>().FirstOrDefault();
            return implementationType != null && typeof(T) != typeof(IDataComponent);
        }

        private bool TryFindImplementation<T>(string implementationTypeName,
            out Type implementationType) where T : IDataComponent
        {
            implementationType =
                FindImplementations<T>()
                .FirstOrDefault(t => t.Name.ToLower() == implementationTypeName);

            return implementationType != null && typeof(T) != typeof(IDataComponent);
        }

        private IEnumerable<Type> FindImplementations<T>() where T : IDataComponent
        {
            Type interfaceType = typeof(T);

            return (from type in implementationsAssembly.GetTypes()
                    where interfaceType.IsAssignableFrom(type)
                    && type.IsInterface == false
                    select type);
        }

        private static void ThrowNotImplementedTypeException<T>() where T : IDataComponent
        {
            throw new NotImplementedException(string.Format(errMsgNotImplemented,
                                    typeof(T).ToString()));
        }

        private bool TryToCreateInstance<T>(Type outputType, out T instance)
        {
            instance = (T)implementationsAssembly.CreateInstance(outputType.FullName);
            return instance != null;
        }       

        private static void ThrowErrorOnTypeLoadException<T>() where T : IDataComponent
        {
            string errorMessage = string.Format(loadImplementationError, typeof(T).FullName);
            throw new ReflectionTypeLoadException(null, null, errorMessage);
        }
    }
}