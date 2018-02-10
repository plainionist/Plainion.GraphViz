using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services.Framework
{
    [Serializable]
    public class TypeDescriptor
    {
        private int myHashCode;

        public TypeDescriptor(Type type)
        {
            if (type.IsGenericType)
            {
                myHashCode = type.GetGenericTypeDefinition().GetHashCode();
            }
            else
            {
                myHashCode = type.GetHashCode();
            }

            // Hint: encode the fullname into the Id of the nodes to allow filtering by Id 
            Id = type.FullName == null ? myHashCode.ToString() : type.FullName + "#" + myHashCode;

            Name = type.Name;
            // TODO: fullname might be null!!!
            FullName = type.FullName;
        }

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
    }
}
