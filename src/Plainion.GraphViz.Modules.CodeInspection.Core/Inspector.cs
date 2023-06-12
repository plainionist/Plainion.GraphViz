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
            Contract.RequiresNotNull(loader, "loader");
            Contract.RequiresNotNull(type, "type");

            myLoader = loader;
            myType = type;

            // .net full-names do escape "."
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
                .Select(e => new Reference(myType, e.To.GetElementType(), e.ReferenceType));

            edges = edges.Concat(elementTypes)
                .ToList();

            var genericTypes = edges
                .Where(e => e.To.IsGenericType)
                .SelectMany(e => e.To.GetGenericArguments().Select(t => new Reference(myType, t, e.ReferenceType)));

            return edges.Concat(genericTypes)
                .ToList();
        }

        private IEnumerable<Reference> GetBaseTypes()
        {
            var baseType = myType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                yield return new Reference(myType, baseType, ReferenceType.DerivesFrom);

                baseType = baseType.BaseType;
            }
        }

        private IEnumerable<Reference> GetInterfaces()
        {
            return myType.GetInterfaces()
                .Select(t => new Reference(myType, t, ReferenceType.Implements));
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
                return new Reference(myType, extractor(), ReferenceType.Undefined);
            }
            catch (Exception ex)
            {
                FailedToExtractTypeFromMember(ex);
                return null;
            }
        }

        private void FailedToExtractTypeFromMember(Exception ex)
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
                types = types.Concat(method.GetGenericArguments().Select(t => new Reference(myType, t, ReferenceType.Undefined)));
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
            catch (Exception ex)
            {
                FailedToExtractTypeFromMember(ex);
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
            return GetMethods()
                .SelectMany(GetCalledTypes)
                .Where(r => r != null)
                .ToList();
        }

        //http://stackoverflow.com/questions/4184384/mono-cecil-typereference-to-type
        private IEnumerable<MethodDefinition> GetMethods()
        {
            try
            {
                if (myType.IsInterface || !myType.IsClass)
                {
                    return Enumerable.Empty<MethodDefinition>();
                }
            }
            catch (FileNotFoundException ex)
            {
                FailedToExtractTypeFromMember(ex);
                return Enumerable.Empty<MethodDefinition>();
            }

            // don't use the ".Types" property - it does not return public nested types
            var cecilTypes = myLoader.MonoLoad(myType.Assembly).MainModule.GetTypes();

            var cecilType = cecilTypes
                .SingleOrDefault(t => t.FullName == myFullName);

            if (cecilType == null)
            {
                // Mono.Ceceil uses '/' as separator instead '+'
                var nestedFullName = myFullName.Replace('+', '/');

                cecilType = cecilTypes
                    .SingleOrDefault(t => t.FullName == nestedFullName);

                if (cecilType == null)
                {
                    Console.Write("!{0}!", myFullName);

                    return Enumerable.Empty<MethodDefinition>();
                }
            }

            return cecilType.Methods
                .Where(m => m.HasBody);
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
                        yield return TryCreateReference(typeRef);
                    }
                }
                else if (instr.OpCode == OpCodes.Newobj)
                {
                    var methodRef = instr.Operand as MethodReference;
                    if (methodRef != null)
                    {
                        yield return TryCreateReference(methodRef.DeclaringType);
                    }
                }
                else if (instr.OpCode == OpCodes.Newarr)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return TryCreateReference(typeRef);
                    }
                }
                else if (instr.OpCode == OpCodes.Castclass)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return TryCreateReference(typeRef);
                    }
                }
                else if (instr.OpCode == OpCodes.Isinst)
                {
                    var typeRef = instr.Operand as TypeReference;
                    if (typeRef != null)
                    {
                        yield return TryCreateReference(typeRef);
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

                    yield return TryCreateReference(callee.DeclaringType, ReferenceType.Calls);

                    yield return TryCreateReference(callee.ReturnType);

                    if (callee.HasGenericParameters)
                    {
                        foreach (var parameter in callee.GenericParameters)
                        {
                            yield return TryCreateReference(parameter);
                        }
                    }

                    if (callee.IsGenericInstance)
                    {
                        foreach (var parameter in ((GenericInstanceMethod)callee).GenericArguments)
                        {
                            yield return TryCreateReference(parameter);
                        }
                    }

                    if (callee.HasParameters)
                    {
                        foreach (var parameter in callee.Parameters)
                        {
                            yield return TryCreateReference(parameter.ParameterType);
                        }
                    }
                }
            }
        }

        private Reference TryCreateReference(TypeReference to, ReferenceType refType = ReferenceType.Undefined)
        {
            var dotNetType = TryGetSystemType(to);
            return dotNetType != null ? new Reference(myType, dotNetType, refType) : null;
        }

        private Type TryGetSystemType(TypeReference to, ReferenceType refType = ReferenceType.Undefined)
        {
            if (to.FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var dotNetType = myLoader.FindTypeByName(to);
            if (dotNetType != null)
            {
                return dotNetType;
            }

            return null;
        }

        /// <summary>
        /// Does not cover calls of constructors.
        /// </summary>
        public IEnumerable<MethodCall> GetCalledMethods()
        {
            return GetMethods()
                .SelectMany(GetMethodCalls)
                .ToList();
        }

        private IEnumerable<MethodCall> GetMethodCalls(MethodDefinition method)
        {
            var caller = new Method(myType, method.Name);
            foreach (var instr in method.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt || instr.OpCode == OpCodes.Calli)
                {
                    var site = instr.Operand as CallSite;
                    if (site != null)
                    {
                        // C++/CLI ?
                        continue;
                    }

                    var calledMethod = ((MethodReference)instr.Operand);
                    var dotNetType = TryGetSystemType(calledMethod.DeclaringType);
                    if (dotNetType != null)
                    {
                        yield return new MethodCall(caller, new Method(dotNetType, calledMethod.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Returns all hard coded strings
        /// </summary>
        public IEnumerable<string> GetHardcodedStrings()
        {
            return GetMethods()
                .SelectMany(GetHardcodedStrings)
                .ToList();
        }

        private IEnumerable<string> GetHardcodedStrings(MethodDefinition method)
        {
            var caller = new Method(myType, method.Name);
            foreach (var instr in method.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldstr)
                {
                    yield return (string)instr.Operand;
                }
            }
        }
    }
}
