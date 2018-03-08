using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Yuki.Core.Resolver.Infrastructure;

namespace Yuki.Core.Resolver
{
   class Injector : Singleton<Injector>
   {
      private Injector()
      {

      }

      private static Injector InitializeInstance()
      {
         return new Injector();
      }

      public bool TryInjectTypes<T>(T instance, out T initializedInstance)
      {
         initializedInstance = default(T);

         if (TryGetInjectedFieldsAndTypes(instance.GetType(),
            out IEnumerable<(Type type, string name)> injectedTypes))
         {
            injectedTypes.ToList().ForEach(fieldInjectionData =>
            {
               SetInjectedFieldToInstance(ref instance, fieldInjectionData);
            });

            initializedInstance = instance;
         }

         return initializedInstance != null;
      }

      private void SetInjectedFieldToInstance<T>(ref T instance,
         (Type type, string name) fieldInjectionData)
      {
         Type type = instance.GetType();

         var method = typeof(Resolver)
            .GetMethods().Where(m => m.GetParameters().Count() == 0).FirstOrDefault();

         var genericMethod = method.MakeGenericMethod(fieldInjectionData.type);
         var injectedInstance = genericMethod.Invoke(Resolver.GetInstance(), new object[0]);

         AddLazyInitializationMethodToInstanceAtRuntime<T>(ref instance, injectedInstance,
            fieldInjectionData.name);

         FieldInfo fieldInfo = type.GetField(fieldInjectionData.name,
           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
           BindingFlags.Instance | BindingFlags.IgnoreCase);

         fieldInfo.SetValue(instance, injectedInstance);
      }

      bool TryGetInjectedFieldsAndTypes(Type implementationType,
         out IEnumerable<(Type type, string name)> injectedTypes)
      {
         object[] parameters = { new List<(Type type, string name)>() };

         //invoked via reflection to be able to pass in runtime the Type to the generic method
         var method = typeof(Injector).GetMethod("TryGetInjectedInfo",
            BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

         var genericMethod = method.MakeGenericMethod(implementationType);

         bool invocationResult = (bool)genericMethod.Invoke(this, parameters);

         injectedTypes = (IEnumerable<(Type type, string name)>)parameters[0];

         return invocationResult;
      }

      bool TryGetInjectedInfo<T>(out IEnumerable<(Type type, string name)> injectedTypes)
      {
         injectedTypes = GetPropertiesTypes<T>();
         return true;
      }

      IEnumerable<(Type type, string name)> GetPropertiesTypes<T>()
      {
         Type type = typeof(T);

         PropertyInfo[] propertiesInfo = type
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);

         return propertiesInfo.ToList()
            .Where(p => p.GetCustomAttribute(typeof(Inject)) != null)
            .Select(p => (type: p.PropertyType, name: p.Name));
      }

      private void AddLazyInitializationMethodToInstanceAtRuntime<T>(ref T instance,
         object injectedInstance, string propertyName)
      {
         var method = typeof(Injector)
            .GetMethod("AddLazyInitializationMethodToInstance",
            BindingFlags.Instance | BindingFlags.NonPublic);

         Type instanceType = instance.GetType();
         Type injectedInstanceType = injectedInstance.GetType().UnderlyingSystemType;


         var genericMethod = method
            .MakeGenericMethod(new Type[] { instanceType,
               injectedInstanceType.UnderlyingSystemType });

         genericMethod.Invoke(this, new object[] { instance, injectedInstance, propertyName });

      }

      private void AddLazyInitializationMethodToInstance<T, Tinjected>(ref T instance,
         Tinjected injectedInstance, string propertyName)
      {
         var init = GetType().GetMethod("GetInstanceLazily");
         var genericMethod = init.MakeGenericMethod(new Type[] { typeof(Tinjected) });

         var propMethod = new DynamicMethod("AccessProp", typeof(Tinjected), new Type[0]);

         ILGenerator ilGenerator = propMethod.GetILGenerator();

         ilGenerator.Emit(OpCodes.Call, init);
         ilGenerator.Emit(OpCodes.Ret);

         PropertyInfo propertyInfo = instance.GetType().GetProperty(propertyName);

         //MethodInfo methodInfo = GetMethod(typeof(T), typeof(T).GetProperty(propertyName).GetMethod);

         //metho

         //propertyInfo.SetValue(instance, GetInstanceLazily());

         //https://stackoverflow.com/questions/8459188/emitting-il-to-call-a-math-function

      }

      private static T GetInstanceLazily<T>(T injectedInstance)
      {
         Lazy<T> lazyIntance = new Lazy<T>(() => injectedInstance);
         return lazyIntance.Value;
      }
   }
}
