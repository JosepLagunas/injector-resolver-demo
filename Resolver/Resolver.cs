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
      private IDictionary<string, Mapping> typesMapping;

      private List<Type> implementationsTypes;
      private bool mappingAllowAutoRegisterForSingleTypes;

      private const string errMsgNotImplemented = "{0} not implemented.";
      private const string loadImplementationError = "Unable to create instance of Type {0}";
      private const string notRegisteredTypeError = "Not Registered Type found for {0}";

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
         mappingAllowAutoRegisterForSingleTypes = false;

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
            AppContext.BaseDirectory, MAPPINGS_CONFIG_FILE_PATH);

         MappingConfiguration mappingConfiguration =
                 MappingConfiguration.LoadMappingConfiguration(configurationFilePath);

         mappingAllowAutoRegisterForSingleTypes =
             mappingConfiguration.AllowAutoRegisterOfSingleTypes;

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
         typesMapping = new Dictionary<string, Mapping>();
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

         MultiMapping mapping = new MultiMapping
         {
            Interface = "IVat",
            MultiImplementation = true,
            Implementations = new List<SpecificImplementation>()
            {
                new SpecificImplementation(){ Discriminator = Country.Belgium,
                    Implementation = "VatBelgium"},
                new SpecificImplementation(){ Discriminator = Country.Netherlands,
                    Implementation = "VatNetherlands"},
                new SpecificImplementation(){ Discriminator = Country.Spain,
                    Implementation = "VatSpain"}
            }
         };
         typesMapping.Add(typeof(IVat).Name.ToLower(), mapping);
      }

      private void DoMappings(IEnumerable<Mapping> mappings)
      {
         mappings.ToList().ForEach(mapping =>
         {
            if (mapping.MultiImplementation)
            {
               typesMapping.Add(mapping.Interface.ToLower(), (MultiMapping)mapping);
            }
            else
            {
               typesMapping.Add(mapping.Interface.ToLower(), (SingleMapping)mapping);
            }
         });
      }

      public static T Resolve<T>() where T : IDataComponent
      {
         try
         {
            Resolver resolver = Resolver.GetInstance();

            T implementation = resolver
                .GetImplementation<T>(resolver.mappingAllowAutoRegisterForSingleTypes);
            
            return implementation;

         }
         catch (NotImplementedException e)
         {
            //Handle exception, out of scoope now.
            throw e;
         }
         catch (ReflectionTypeLoadException e)
         {
            //Handle exception, out of scope now.
            throw e;
         }
         catch (Exception e)
         {
            //Handle exception, out of scope now.
            throw e;
         }
      }

      public static T Resolve<T>(Country country) where T : IDataComponent
      {
         try
         {
            Resolver resolver = GetInstance();
            var implementation = resolver.GetImplementation<T>(country);
                        
            return implementation;

         }
         catch (NotImplementedException e)
         {
            //Handle exception, out of scope now.
            throw e;
         }
         catch (ReflectionTypeLoadException e)
         {
            //Handle exception, out of scope now.
            throw e;
         }
         catch (Exception e)
         {
            //Handle exception, out of scope now.
            throw e;
         }
      }

      private T GetImplementation<T>(bool allowAutoRegisterOfTypes) where T : IDataComponent
      {
         if (!allowAutoRegisterOfTypes
             && !TryToGetImplementationTypeNameFromMappingConfig<T>(out string implTypeName))
         {
            ThrowNoRegisteredTypeException<T>();
         }

         if (!TryFindImplementation<T>(out Type implementationType))
         {
            ThrowNotImplementedTypeException<T>();
         }

         Type proxyType = ProxyFactory.GetInstance()
            .BuildProxyType(implementationType);

         return BuildInstance<T>(proxyType);         
      }

      private T GetImplementation<T>(Country country) where T : IDataComponent
      {
         if (!TryToGetImplementationTypeNameFromMappingConfig<T>(country,
                 out string implementationTypeName))
         {
            ThrowNoRegisteredTypeException<T>();
         }

         if (!TryFindImplementation<T>(implementationTypeName, out Type implementationType))
         {
            ThrowNotImplementedTypeException<T>();
         }

         Type proxyType = ProxyFactory.GetInstance()
            .BuildProxyType(implementationType);

         return BuildInstance<T>(proxyType);
      }

      private static void ThrowNoRegisteredTypeException<T>()
      {
         throw new NotSupportedException(string.Format(notRegisteredTypeError, typeof(T).Name));
      }

      private static bool TryToGetImplementationTypeNameFromMappingConfig<T>(Country country,
          out string implementationTypeName) where T : IDataComponent
      {
         string interfaceName = typeof(T).Name;

         MultiMapping mapping =
             (MultiMapping)GetInstance().typesMapping[interfaceName.ToLower()];

         implementationTypeName = mapping.Implementations
             .FirstOrDefault(impl => impl.Discriminator == country).Implementation;

         return implementationTypeName != null;
      }
      private static bool
          TryToGetImplementationTypeNameFromMappingConfig<T>(out string implementationTypeName)
          where T : IDataComponent
      {
         string interfaceName = typeof(T).Name;

         bool found = GetInstance()
            .typesMapping.TryGetValue(interfaceName.ToLower(), out Mapping mapping);

         if (!found)
         {
            ThrowNoRegisteredTypeException<T>();
         }

         implementationTypeName = ((SingleMapping)mapping).Implementation;

         return implementationTypeName != null;
      }
      private static T BuildInstance<T>(Type implementationType) where T : IDataComponent
      {
         if (!InstanceBuilder.GetInstance()
                         .TryToCreateInstance(implementationType, out T instance))
         {
            ThrowErrorOnTypeLoadException<T>();
         }

         return instance;
      }

      internal bool TryFindImplementation<T>(out Type implementationType) where T : IDataComponent
      {
         implementationType = FindImplementations<T>().FirstOrDefault();
         return implementationType != null && typeof(T) != typeof(IDataComponent);
      }

      internal bool TryFindImplementation<T>(string implementationTypeName,
          out Type implementationType) where T : IDataComponent
      {
         implementationTypeName = implementationTypeName.ToLower();
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

      private static void ThrowErrorOnTypeLoadException<T>() where T : IDataComponent
      {
         string errorMessage = string.Format(loadImplementationError, typeof(T).FullName);
         throw new ReflectionTypeLoadException(null, null, errorMessage);
      }
   }
}