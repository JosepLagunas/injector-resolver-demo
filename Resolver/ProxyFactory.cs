using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Yuki.Core.Resolver.Infrastructure;

namespace Yuki.Core.Resolver
{
   public class ProxyFactory : Singleton<ProxyFactory>
   {

      private ProxyFactory()
      {

      }

      private static ProxyFactory InitializeInstance()
      {
         return new ProxyFactory();
      }
            
      public Type BuildProxyType(Type baseType)
      {
         AssemblyBuilder assemblyBuilder = GetDynamicAssembly();
         ModuleBuilder moduleBuilder = BuildModuleBuilder(assemblyBuilder);
         TypeBuilder typeBuilder = BuildTypeBuilderFromType(moduleBuilder, baseType);
         Type ProxyType = BuildProxyType(baseType, typeBuilder);

         return ProxyType;
      }

      private AssemblyBuilder GetDynamicAssembly()
      {
         AssemblyName assemblyName = new AssemblyName("Proxy.Implementations");
         AssemblyBuilder assemblyBuilder =
             AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

         return assemblyBuilder;
      }

      private ModuleBuilder BuildModuleBuilder(AssemblyBuilder assemblyBuilder)
      {
         ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
         return moduleBuilder;
      }

      private TypeBuilder BuildTypeBuilderFromType(ModuleBuilder moduleBuilder, Type type)
      {
         string proxyTypeName = $"{type.Name}_proxy";
         TypeAttributes typeAttributes = type.Attributes;
         return moduleBuilder.DefineType(proxyTypeName, typeAttributes);
      }

      private Type BuildProxyType(Type baseType, TypeBuilder typeBuilder)
      {
         Type proxyType = BuildType(baseType, ref typeBuilder);

         return proxyType;
      }

      private Type BuildType(Type baseType, ref TypeBuilder typeBuilder)
      {
         AddPublicMembers(baseType, ref typeBuilder);
         return typeBuilder.AsType();
      }

      private void AddPublicMembers(Type baseType, ref TypeBuilder typeBuilder)
      {
         AddPublicFields(baseType, ref typeBuilder);
         AddPublicProperties(baseType, ref typeBuilder);
         AddPublicMethods(baseType, ref typeBuilder);
      }

      private void AddPublicFields(Type baseType, ref TypeBuilder typeBuilder)
      {
         IList<FieldInfo> fieldsInfo = baseType.GetFields();
         foreach(FieldInfo fieldInfo in fieldsInfo)
         {
            typeBuilder.DefineField(fieldInfo.Name, fieldInfo.FieldType, fieldInfo.Attributes);
         }
      }

      private void AddPublicProperties(Type baseType, ref TypeBuilder typeBuilder)
      {
         IList<PropertyInfo> propertiesInfo = baseType.GetProperties();
         foreach(PropertyInfo propertyInfo in propertiesInfo)
         {
            var parametersTypes = propertyInfo.GetMethod.GetParameters()
               .Select(pi => pi.ParameterType).ToArray();

            //method definition
            PropertyBuilder pBuilder = typeBuilder.DefineProperty(propertyInfo.Name,
               propertyInfo.Attributes, propertyInfo.PropertyType,
               parametersTypes);
            
            //method body to be defined here
            
            //new method overriding here invoking parent method
           
         }
      }

      private void AddPublicMethods(Type baseType, ref TypeBuilder type)
      {
         throw new NotImplementedException();
      }
   }
}