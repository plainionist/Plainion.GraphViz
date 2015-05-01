using System;

namespace Plainion.GraphViz.Modules.Reflection.Services.Framework
{
    [Serializable]
    public class TypeDescriptor 
    {
        private int myHashCode;

        public TypeDescriptor( Type type )
        {
            myHashCode = type.GetHashCode();

            Id = myHashCode.ToString();
            Name = type.Name;
            // TODO: fullname might be null!!!
            FullName = type.FullName;
        }

        public string Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string FullName
        {
            get;
            private set;
        }

        public override int GetHashCode()
        {
            return myHashCode;
        }

        public override bool Equals( object obj )
        {
            var other = obj as TypeDescriptor;
            if( other == null )
            {
                return false;
            }

            return myHashCode == other.myHashCode;
        }
    }
}
