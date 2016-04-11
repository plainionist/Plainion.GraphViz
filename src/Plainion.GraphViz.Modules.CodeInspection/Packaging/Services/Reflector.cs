using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
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

        internal IEnumerable<Edge> GetUsedTypes()
        {
            IEnumerable<Edge> edges = GetBaseTypes()
                .Concat(GetInterfaces())
                .Concat(GetFieldTypes())
                .Concat(GetPropertyTypes())
                .Concat(GetMethodTypes())
                .Concat(GetConstructorTypes())
                .Concat(GetCalledTypes())
                .Distinct()
                .ToList();

            var elementTypes = edges
                .Where(e => e.Target.HasElementType)
                .Select(e => new Edge { Target = e.Target.GetElementType(), EdgeType = e.EdgeType });

            edges = edges.Concat(elementTypes)
                .ToList();

            var genericTypes = edges
                .Where(e => e.Target.IsGenericType)
                .SelectMany(e => e.Target.GetGenericArguments().Select(t => new Edge { Target = t, EdgeType = e.EdgeType }));

            edges = edges.Concat(genericTypes)
                .ToList();

            foreach (var edge in edges)
            {
                edge.Source = myType;
            }

            return edges;
        }

        private IEnumerable<Edge> GetBaseTypes()
        {
            var baseType = myType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                yield return new Edge { Target = baseType, EdgeType = EdgeType.DerivesFrom };

                baseType = baseType.BaseType;
            }
        }

        private IEnumerable<Edge> GetInterfaces()
        {
            return myType.GetInterfaces()
                .Select(t => new Edge { Target = t, EdgeType = EdgeType.Implements });
        }

        private IEnumerable<Edge> GetFieldTypes()
        {
            return myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Select(field => new Edge { Target = field.FieldType });
        }

        private IEnumerable<Edge> GetPropertyTypes()
        {
            return myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Select(property => new Edge { Target = property.PropertyType });
        }

        private IEnumerable<Edge> GetMethodTypes()
        {
            return myType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SelectMany(GetMethodTypes);
        }

        private IEnumerable<Edge> GetMethodTypes(MethodInfo method)
        {
            IEnumerable<Edge> types = new[] { new Edge { Target = method.ReturnType } };

            if (method.IsGenericMethod)
            {
                types = types.Concat(method.GetGenericArguments().Select(t => new Edge { Target = t }));
            }

            return types.Concat(GetParameterTypes(method));
        }

        private IEnumerable<Edge> GetParameterTypes(MethodBase method)
        {
            return method.GetParameters()
                .Select(p => new Edge { Target = p.ParameterType });
        }

        private IEnumerable<Edge> GetConstructorTypes()
        {
            return myType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SelectMany(GetParameterTypes);
        }

        //http://stackoverflow.com/questions/4184384/mono-cecil-typereference-to-type
        private IEnumerable<Edge> GetCalledTypes()
        {
            if (myType.IsInterface || myType.IsNested || !myType.IsClass)
            {
                return Enumerable.Empty<Edge>();
            }

            var cecilType = myLoader.MonoLoad(myType.Assembly).MainModule.Types
                .SingleOrDefault(t => t.FullName == myFullName);

            if (cecilType == null)
            {
                Console.Write("!{0}!", myFullName);

                return Enumerable.Empty<Edge>();
            }

            return cecilType.Methods
                .Where(m => m.HasBody)
                .SelectMany(GetCalledTypes)
                .Where(edge => edge.Target != null)
                .ToList();
        }

        // https://msdn.microsoft.com/de-de/library/system.reflection.emit.opcodes(v=vs.110).aspx
        private IEnumerable<Edge> GetCalledTypes(MethodDefinition method)
        {
            foreach (var instr in method.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldtoken)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return new Edge { Target = TryGetSystemType(typeRef) };
                    }
                }
                else if( instr.OpCode == OpCodes.Newobj )
                {
                    var methodRef = instr.Operand as MethodReference;
                    if( methodRef != null )
                    {
                        yield return new Edge { Target = TryGetSystemType( methodRef.DeclaringType ) };
                    }
                }
                else if( instr.OpCode == OpCodes.Newarr )
                {
                    var typeRef = instr.Operand as TypeReference;
                    if( typeRef != null )
                    {
                        yield return new Edge { Target = TryGetSystemType( typeRef ) };
                    }
                }
                else if( instr.OpCode == OpCodes.Castclass )
                {
                    var typeRef = instr.Operand as TypeReference;
                    if( typeRef != null )
                    {
                        yield return new Edge { Target = TryGetSystemType( typeRef ) };
                    }
                }
                else if( instr.OpCode == OpCodes.Isinst )
                {
                    var typeRef = instr.Operand as TypeReference;
                    if( typeRef != null )
                    {
                        yield return new Edge { Target = TryGetSystemType( typeRef ) };
                    }
                }
                else if( instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt || instr.OpCode == OpCodes.Calli )
                {
                    var site = instr.Operand as CallSite;
                    if (site != null)
                    {
                        // C++/CLI ?
                        continue;
                    }

                    var callee = ((MethodReference)instr.Operand);

                    yield return new Edge { Target = TryGetSystemType(callee.DeclaringType), EdgeType = EdgeType.Calls };

                    yield return new Edge { Target = TryGetSystemType(callee.ReturnType) };

                    if (callee.HasGenericParameters)
                    {
                        foreach (var parameter in callee.GenericParameters)
                        {
                            yield return new Edge { Target = TryGetSystemType(parameter) };
                        }
                    }

                    if (callee.IsGenericInstance)
                    {
                        foreach (var parameter in ((GenericInstanceMethod)callee).GenericArguments)
                        {
                            yield return new Edge { Target = TryGetSystemType(parameter) };
                        }
                    }

                    if (callee.HasParameters)
                    {
                        foreach (var parameter in callee.Parameters)
                        {
                            yield return new Edge { Target = TryGetSystemType(parameter.ParameterType) };
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
