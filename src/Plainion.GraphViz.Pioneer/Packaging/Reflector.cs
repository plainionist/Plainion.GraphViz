using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace Plainion.GraphViz.Pioneer.Packaging
{
    // http://stackoverflow.com/questions/24680054/how-to-get-the-list-of-methods-called-from-a-method-using-reflection-in-c-sharp
    class Reflector
    {
        private readonly AssemblyLoader myLoader;
        private readonly Type myType;

        public Reflector(AssemblyLoader loader, Type type)
        {
            myLoader = loader;
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
                .Concat(GetCalledTypes())
                .Distinct()
                .ToList();
            //IEnumerable<Type> types = GetCalledTypes()
            //    .Distinct()
            //    .ToList();

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

        //http://stackoverflow.com/questions/4184384/mono-cecil-typereference-to-type
        private IEnumerable<Type> GetCalledTypes()
        {
            if (myType.IsInterface || myType.IsNested || !myType.IsClass)
            {
                yield break;
            }

            var cecilType = myLoader.MonoLoad(myType.Assembly).MainModule.Types
                .SingleOrDefault(t => t.FullName == myType.FullName);

            if (cecilType == null)
            {
                Console.Write("!");
                yield break;
            }

            var methods = cecilType.Methods
                .Where(m => m.HasBody)
                .SelectMany(m => m.Body.Instructions);

            foreach (var instr in methods)
            {
                if (instr.OpCode == OpCodes.Call)
                {
                    var callee = ((MethodReference)instr.Operand);
                    var declaringType = callee.DeclaringType;

                    if (!declaringType.FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
                    {
                        var dotNetType = myLoader.FindTypeByName(declaringType.FullName);
                        if (dotNetType != null)
                        {
                            yield return dotNetType;
                        }
                        else
                        {
                            Console.Write("-");
                        }
                    }
                }
            }
        }
    }
}
