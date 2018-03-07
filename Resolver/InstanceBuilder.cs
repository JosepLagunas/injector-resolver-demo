using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Yuki.Core.Resolver.Infrastructure;

namespace Yuki.Core.Resolver
{
   class InstanceBuilder : Singleton<InstanceBuilder>
   {
      private delegate dynamic CreateInstanceDelegate();
      private IDictionary<Type, CreateInstanceDelegate> constructors;

      private InstanceBuilder()
      {
         constructors = new Dictionary<Type, CreateInstanceDelegate>();
      }

      private static InstanceBuilder InitializeInstance()
      {
         return new InstanceBuilder();
      }

      public bool TryToCreateInstance<T>(Type outputType, out T instance)
      {
         if (!GetInstance().constructors.TryGetValue(outputType,
             out CreateInstanceDelegate constructorDelegate))
         {
            lock (outputType)
            {
               if (!GetInstance().constructors.TryGetValue(outputType,
                   out constructorDelegate))
               {
                  constructorDelegate = BuildConstructorDelegate(outputType);

                  constructors.Add(outputType, constructorDelegate);
               }
            }
         }

         instance = (T)constructorDelegate();

         return instance != null;
      }

      private CreateInstanceDelegate BuildConstructorDelegate(Type outputType)
      {
         CreateInstanceDelegate constructorDelegate;
         DynamicMethod dynamicMethod = new DynamicMethod("Constructor_" + outputType.Name
             , outputType, new Type[0]);

         ConstructorInfo constructorInfo = outputType.GetConstructor(new Type[0]);
         ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
         iLGenerator.Emit(OpCodes.Newobj, constructorInfo);
         iLGenerator.Emit(OpCodes.Ret);

         constructorDelegate =
             (CreateInstanceDelegate)dynamicMethod
             .CreateDelegate(typeof(CreateInstanceDelegate));

         return constructorDelegate;
      }

      private void AddLazyInitializationMethodToInstance<T, Tinjected>(ref T instance)
      {
         var lazyType = typeof(Lazy<>).MakeGenericType(new[] { typeof(Tinjected) });

         DynamicMethod dynamicMethod =
            new DynamicMethod("LazyInitializator_" + typeof(Tinjected).Name, lazyType, new Type[0]);

         ConstructorInfo constructorInfo = lazyType.GetConstructor(new Type[0]);
         ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
         iLGenerator.Emit(OpCodes.Newobj, constructorInfo);
         iLGenerator.Emit(OpCodes.Ret);

         //constructorDelegate =
         //    (CreateInstanceDelegate)dynamicMethod
         //    .CreateDelegate(typeof(CreateInstanceDelegate));

         //return constructorDelegate;
      }


   }
}
