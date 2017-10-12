using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Plainion.GraphViz.Modules.CodeInspection.Core
{
    /// <summary>
    /// Thread-safe.
    /// Assembly loader is passed from outside because it should be shared across different instances 
    /// of the class.
    /// </summary>
    // http://stackoverflow.com/questions/24680054/how-to-get-the-list-of-methods-called-from-a-method-using-reflection-in-c-sharp
    public class Inspector
    {
        private readonly MonoLoader myLoader;
        private readonly Type myType;
        private readonly string myFullName;

        public Inspector(MonoLoader loader, Type type)
        {
            myLoader = loader;
            myType = type;

            // .net fullnames do escape "."
            myFullName = myType.FullName.Replace("\\,", ",");
        }

        /// <summary>
        /// Returns all types "used" (e.g. called, used in parameters, derived from) by the type
        /// under inspection
        /// </summary>
        public IEnumerable<Reference> GetUsedTypes()
        {
            IEnumerable<Reference> edges = GetBaseTypes()
                .Concat(GetInterfaces())
                .Concat(GetFieldTypes())
                .Concat(GetPropertyTypes())
                .Concat(GetMethodTypes())
                .Concat(GetConstructorTypes())
                .Concat(GetCalledTypes())
                .Distinct()
                .Where(e => e != null)
                .ToList();

            var elementTypes = edges
                .Where(e => e.To.HasElementType)
                .Select(e => new Reference(e.To.GetElementType(), e.ReferenceType));

            edges = edges.Concat(elementTypes)
                .ToList();

            var genericTypes = edges
                .Where(e => e.To.IsGenericType)
                .SelectMany(e => e.To.GetGenericArguments().Select(t => new Reference(t, e.ReferenceType)));

            edges = edges.Concat(genericTypes)
                .ToList();

            foreach (var edge in edges)
            {
                edge.From = myType;
            }

            return edges;
        }

        private IEnumerable<Reference> GetBaseTypes()
        {
            var baseType = myType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                yield return new Reference(baseType, ReferenceType.DerivesFrom);

                baseType = baseType.BaseType;
            }
        }

        private IEnumerable<Reference> GetInterfaces()
        {
            return myType.GetInterfaces()
                .Select(t => new Reference(t, ReferenceType.Implements));
        }

        private IEnumerable<Reference> GetFieldTypes()
        {
            return myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Select(field => SafeCreateEdge(() => field.FieldType));
        }

        private Reference SafeCreateEdge(Func<Type> extractor)
        {
            try
            {
                return new Reference(extractor());
            }
            catch (FileNotFoundException ex)
            {
                FileNotFound(ex);
                return null;
            }
        }

        private void FileNotFound(FileNotFoundException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ERROR: failed to extract type from member.");
            sb.AppendLine("  Type: " + myType.FullName);
            sb.AppendLine("  Exception: " + ex.Message);
            Console.WriteLine(sb.ToString());
        }

        private IEnumerable<Reference> GetPropertyTypes()
        {
            return myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Select(property => SafeCreateEdge(() => property.PropertyType));
        }

        private IEnumerable<Reference> GetMethodTypes()
        {
            return myType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SelectMany(GetMethodTypes);
        }

        private IEnumerable<Reference> GetMethodTypes(MethodInfo method)
        {
            IEnumerable<Reference> types = new[] { SafeCreateEdge(() => method.ReturnType) };

            if (method.IsGenericMethod)
            {
                types = types.Concat(method.GetGenericArguments().Select(t => new Reference(t)));
            }

            return types.Concat(GetParameterTypes(method));
        }

        private IEnumerable<Reference> GetParameterTypes(MethodBase method)
        {
            try
            {
                return method.GetParameters()
                    .Select(p => SafeCreateEdge(() => p.ParameterType));
            }
            catch (FileNotFoundException ex)
            {
                FileNotFound(ex);
                return Enumerable.Empty<Reference>();
            }
        }

        private IEnumerable<Reference> GetConstructorTypes()
        {
            return myType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .SelectMany(GetParameterTypes);
        }

        //http://stackoverflow.com/questions/4184384/mono-cecil-typereference-to-type
        private IEnumerable<Reference> GetCalledTypes()
        {
            try
            {
                if (myType.IsInterface || myType.IsNested || !myType.IsClass)
                {
                    return Enumerable.Empty<Reference>();
                }
            }
            catch (FileNotFoundException ex)
            {
                FileNotFound(ex);
                return Enumerable.Empty<Reference>();
            }

            var cecilType = myLoader.MonoLoad(myType.Assembly).MainModule.Types
                .SingleOrDefault(t => t.FullName == myFullName);

            if (cecilType == null)
            {
                Console.Write("!{0}!", myFullName);

                return Enumerable.Empty<Reference>();
            }

            return cecilType.Methods
                .Where(m => m.HasBody)
                .SelectMany(GetCalledTypes)
                .Where(edge => edge.To != null)
                .ToList();
        }

        // https://msdn.microsoft.com/de-de/library/system.reflection.emit.opcodes(v=vs.110).aspx
        private IEnumerable<Reference> GetCalledTypes(MethodDefinition method)
        {
            foreach (var instr in method.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldtoken)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return new Reference(TryGetSystemType(typeRef));
                    }
                }
                else if (instr.OpCode == OpCodes.Newobj)
                {
                    var methodRef = instr.Operand as MethodReference;
                    if (methodRef != null)
                    {
                        yield return new Reference(TryGetSystemType(methodRef.DeclaringType));
                    }
                }
                else if (instr.OpCode == OpCodes.Newarr)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return new Reference(TryGetSystemType(typeRef));
                    }
                }
                else if (instr.OpCode == OpCodes.Castclass)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return new Reference(TryGetSystemType(typeRef));
                    }
                }
                else if (instr.OpCode == OpCodes.Isinst)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return new Reference(TryGetSystemType(typeRef));
                    }
                }
                else if (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt || instr.OpCode == OpCodes.Calli)
                {
                    var site = instr.Operand as CallSite;
                    if (site != null)
                    {
                        // C++/CLI ?
                        continue;
                    }

                    var callee = ((MethodReference)instr.Operand);

                    yield return new Reference(TryGetSystemType(callee.DeclaringType), ReferenceType.Calls);

                    yield return new Reference(TryGetSystemType(callee.ReturnType));

                    if (callee.HasGenericParameters)
                    {
                        foreach (var parameter in callee.GenericParameters)
                        {
                            yield return new Reference(TryGetSystemType(parameter));
                        }
                    }

                    if (callee.IsGenericInstance)
                    {
                        foreach (var parameter in ((GenericInstanceMethod)callee).GenericArguments)
                        {
                            yield return new Reference(TryGetSystemType(parameter));
                        }
                    }

                    if (callee.HasParameters)
                    {
                        foreach (var parameter in callee.Parameters)
                        {
                            yield return new Reference(TryGetSystemType(parameter.ParameterType));
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
