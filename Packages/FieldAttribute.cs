using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldAttribute : Attribute
    {
        public byte FieldId { get; }

        public FieldAttribute(byte fieldId)
        {
            FieldId = fieldId;
        }
    }
}
