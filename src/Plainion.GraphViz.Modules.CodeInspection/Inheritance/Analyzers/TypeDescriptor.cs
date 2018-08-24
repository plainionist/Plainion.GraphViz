using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers
{
    [Serializable]
    class TypeDescriptor
    {
        private int myHashCode;

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string FullName { get; private set; }

        public override int GetHashCode()
        {
            return myHashCode;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TypeDescriptor;
            if (other == null)
            {
                return false;
            }

            return myHashCode == other.myHashCode;
        }

        public static TypeDescriptor Create(Type type)
        {
            var descriptor = new TypeDescriptor();

            if (type.IsGenericType)
            {
                descriptor.myHashCode = type.GetGenericTypeDefinition().GetHashCode();
            }
            else
            {
                descriptor.myHashCode = type.GetHashCode();
            }

            // Hint: encode the fullname into the Id of the nodes to allow filtering by Id 
            descriptor.Id = type.FullName == null ? descriptor.myHashCode.ToString() : type.FullName + "#" + descriptor.myHashCode;

            descriptor.Name = type.Name;
            // TODO: fullname might be null!!!
            descriptor.FullName = type.FullName;

            return descriptor;
        }
    }
}
