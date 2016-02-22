using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.Pioneer.Packaging
{
    // http://stackoverflow.com/questions/24680054/how-to-get-the-list-of-methods-called-from-a-method-using-reflection-in-c-sharp
    class Reflector
    {
        private readonly Type myType;

        public Reflector(Type type)
        {
            myType = type;
        }

        internal IEnumerable<Type> GetUsedTypes()
        {
            IEnumerable<Type> types = GetBaseTypes()
                .Concat(GetInterfaces())
                .Concat(GetFieldTypes())
                .Concat(GetPropertyTypes())
                .Concat(GetMethodTypes())
                .Concat(GetConstructorTypes())
                .Distinct()
                .ToList();

            var elementTypes = types
                .Where(t => t.HasElementType)
                .Select(t => t.GetElementType());

            types = types.Concat(elementTypes)
                .ToList();

            var genericTypes = types
                .Where(t => t.IsGenericType)
                .SelectMany(t => t.GetGenericArguments());

            types = types.Concat(genericTypes)
                .ToList();

            return types;
        }

        private IEnumerable<Type> GetBaseTypes()
        {
            var baseType = myType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        private IEnumerable<Type> GetInterfaces()
        {
            return myType.GetInterfaces();
        }

        private IEnumerable<Type> GetFieldTypes()
        {
            return myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Select(field => field.FieldType);
        }

        private IEnumerable<Type> GetPropertyTypes()
        {
            return myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Select(property => property.PropertyType);
        }

        private IEnumerable<Type> GetMethodTypes()
        {
            return myType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SelectMany(GetMethodTypes);
        }

        private IEnumerable<Type> GetMethodTypes(MethodInfo method)
        {
            IEnumerable<Type> types = new[] { method.ReturnType };

            if (method.IsGenericMethod)
            {
                types = types.Concat(method.GetGenericArguments());
            }

            return types.Concat(GetParameterTypes(method));
        }

        private IEnumerable<Type> GetParameterTypes(MethodBase method)
        {
            return method.GetParameters()
                .Select(p => p.ParameterType);
        }

        private IEnumerable<Type> GetConstructorTypes()
        {
            return myType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SelectMany(GetParameterTypes);
        }
    }
}
