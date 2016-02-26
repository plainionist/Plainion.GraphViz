using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Plainion.GraphViz.Pioneer.Services
{
    // http://stackoverflow.com/questions/24680054/how-to-get-the-list-of-methods-called-from-a-method-using-reflection-in-c-sharp
    class Reflector
    {
        private readonly AssemblyLoader myLoader;
        private readonly Type myType;
        private readonly string myFullName;

        public Reflector(AssemblyLoader loader, Type type)
        {
            myLoader = loader;
            myType = type;

            // .net fullnames do escape "."
            myFullName = myType.FullName.Replace("\\,", ",");
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
                return Enumerable.Empty<Type>();
            }

            var cecilType = myLoader.MonoLoad(myType.Assembly).MainModule.Types
                .SingleOrDefault(t => t.FullName == myFullName);

            if (cecilType == null)
            {
                Console.Write("!{0}!", myFullName);

                return Enumerable.Empty<Type>();
            }

            var methods = cecilType.Methods
                .Where(m => m.HasBody)
                .SelectMany(m => m.Body.Instructions);

            return GetCalledTypes(methods)
                .Where(t => t != null)
                .ToList();
        }

        private IEnumerable<Type> GetCalledTypes(IEnumerable<Instruction> methods)
        {
            foreach (var instr in methods)
            {
                if (instr.OpCode == OpCodes.Ldtoken)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return TryGetSystemType(typeRef);
                    }
                }
                else if (instr.OpCode == OpCodes.Call)
                {
                    var callee = ((MethodReference)instr.Operand);
                    var declaringType = callee.DeclaringType;

                    yield return TryGetSystemType(declaringType);

                    yield return  TryGetSystemType(callee.ReturnType);

                    if (callee.HasGenericParameters)
                    {
                        foreach (var parameter in callee.GenericParameters)
                        {
                            yield return TryGetSystemType(parameter);
                        }
                    }

                    if (callee.HasParameters)
                    {
                        foreach (var parameter in callee.Parameters)
                        {
                            yield return TryGetSystemType(parameter.ParameterType);
                        }
                    }
                }
            }
        }

        private Type TryGetSystemType(TypeReference typeRef)
        {
            if (typeRef.FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var dotNetType = myLoader.FindTypeByName(typeRef);
            if (dotNetType != null)
            {
                return dotNetType;
            }

            return null;
        }
    }
}
