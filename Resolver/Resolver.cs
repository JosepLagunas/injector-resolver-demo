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
        private IDictionary<string, IDictionary<Country, string>> implementationsMapping;

        private List<Type> implementationsTypes;

        private const string errMsgNotImplemented = "{0} not implemented.";
        private const string loadImplementationError = "Unable to create instance of Type {0}";

        private const string MAPPINGS_CONFIG_FILE_PATH = 
			   "..\\..\\..\\resolver\\resolver-mapping-config.json";

        private const string IMPLEMENTATIONS_ASSEMBLY_PATH =
            "..\\..\\..\\Yuki.Core.Implementations\\obj\\Debug\\Yuki.Core.Implementations.dll";

        private const string USER_IMPLEMENTATION_ASSEMBLY_PATH =
            "..\\..\\..\\Yuki.Core.Implementations.User\\obj\\" +
            "Debug\\netstandard2.0\\Yuki.Core.Implementations.User.dll";

        private Resolver(bool useConfigurationFile)
        {
            InitializeMappingsDictionary();
            
            if (useConfigurationFile)
            {
                SetConfigurationFromFile();
            }
            else
            {
                SetConfiguration();
            }
        }

        private void SetConfigurationFromFile()
        {
			   string basePath = AppContext.BaseDirectory;

            string configurationFilePath = string.Format("{0}{1}", 
					AppContext.BaseDirectory,MAPPINGS_CONFIG_FILE_PATH);

            MappingConfiguration mappingConfiguration =
                    MappingConfiguration.LoadMappingConfiguration(configurationFilePath);

            IEnumerable<string> absoluteAssembliesPaths =
                mappingConfiguration.AssembliesFiles.ToList()
                .Select(path => basePath + path);

            IEnumerable<Assembly> assemblies =
                LoadImplementationsAssemblies(absoluteAssembliesPaths);

            LoadImplementationsTypes(assemblies);
            DoMappings(mappingConfiguration.Mappings);
        }

        private void SetConfiguration()
        {
            string basePath = AppContext.BaseDirectory;

            IEnumerable<Assembly> assemblies =
                    LoadImplementationsAssemblies(new List<string>{
                        basePath + IMPLEMENTATIONS_ASSEMBLY_PATH,
                        basePath + USER_IMPLEMENTATION_ASSEMBLY_PATH });

            LoadImplementationsTypes(assemblies);
            DoMappings();
        }

        private void InitializeMappingsDictionary()
        {
            implementationsMapping = new Dictionary<string, IDictionary<Country, string>>();
        }

        private void LoadImplementationsTypes(IEnumerable<Assembly> assemblies)
        {
            implementationsTypes = (from assembly in assemblies
                                    select assembly.GetTypes())
                                    .SelectMany(c => c).ToList();
        }

        private MappingConfiguration LoadMappingConfigurationFile(string path)
        {
            string mappingConfigFilePath =
                string.Format("{0}{1}", AppContext.BaseDirectory, MAPPINGS_CONFIG_FILE_PATH);

            return MappingConfiguration.LoadMappingConfiguration(mappingConfigFilePath);
        }

        private IEnumerable<Assembly>
            LoadImplementationsAssemblies(IEnumerable<string> assembliesFilePaths)
        {
            IList<Assembly> assemblies = new List<Assembly>();

            assembliesFilePaths.ToList()
                .ForEach(path => assemblies.Add(Assembly.LoadFile(path)));

            return assemblies;
        }

        private static Resolver InitializeInstance()
        {
            return new Resolver(true);
        }

        private void DoMappings()
        {
            var VatImplementationsDic = new Dictionary<Country, string>
            {
                { Country.Belgium, "VATBelgium" },
                { Country.Netherlands, "VATNetherlands" },
                { Country.Spain, "VATSpain" }
            };
            implementationsMapping.Add(typeof(IVat).Name, VatImplementationsDic);
        }

        private void DoMappings(IEnumerable<Mapping> mappings)
        {
            mappings.ToList()
                .ForEach(mapping =>
                {
                    //ignore single mapping by now. fetched at runtime via Linq.
                    if (mapping.MultiImplementation)
                    {
                        MultiMapping multiMapping = (MultiMapping)mapping;

                        var implementationsDic = new Dictionary<Country, string>();
                        multiMapping.Implementations.ToList<SpecificImplementation>()
                        .ForEach(m => implementationsDic.Add(m.Discriminator, m.Implementation));

                        implementationsMapping.Add(mapping.Interface, implementationsDic);
                    }
                });
        }		  

        public static T Resolve<T>() where T : IDataComponent
        {
            try
            {
                return GetInstance().GetImplementation<T>();
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
            catch (Exception e)
            {
                //Handle exception, out of scoope now.
                throw e;
            }
        }

        public static T Resolve<T>(Country country) where T : IDataComponent
        {
            try
            {
                string interfaceName = typeof(T).Name;
                string implementationName = GetInstance().implementationsMapping[interfaceName][country];
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
            catch (Exception e)
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

        internal bool TryFindImplementation<T>(out Type implementationType) where T : IDataComponent
        {
            implementationType = FindImplementations<T>().FirstOrDefault();
            return implementationType != null && typeof(T) != typeof(IDataComponent);
        }

        internal bool TryFindImplementation<T>(string implementationTypeName,
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

            return (from type in implementationsTypes
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
            instance = (T)outputType.Assembly.CreateInstance(outputType.FullName);
            return instance != null;
        }

        private static void ThrowErrorOnTypeLoadException<T>() where T : IDataComponent
        {
            string errorMessage = string.Format(loadImplementationError, typeof(T).FullName);
            throw new ReflectionTypeLoadException(null, null, errorMessage);
        }
    }
}