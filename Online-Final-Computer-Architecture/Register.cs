using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Online_Final_Computer_Architecture
{
    public class Register : IEquatable<Register>
    {
        public string Name { get; set; }

        public bool Equals([AllowNull] Register other)
        {
            if (other == null)
                return false;
            return this.Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Register objAsPart = obj as Register;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
