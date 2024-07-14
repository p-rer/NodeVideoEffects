using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeVideoEffects.Type
{
    [Serializable]
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException(System.Type existingType, System.Type assignedType)
            : base($"Type mismatch: '{existingType.Name}' <- '{assignedType.Name}'")
        {
        }
    }
}
