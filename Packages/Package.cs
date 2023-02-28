using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public partial class Package
    {
        private Package() { }
        public byte PackageType { get; private set; }
        public byte PackageSubtype { get; private set; }
        public List<PackageField> Fields { get; private set; } = new List<PackageField>();
        public bool Protected { get; private set; }
        public bool ChangeHeaders { get; set; }

        public static Package Create(byte type, byte subtype)
        {
            return new Package() { PackageType = type, PackageSubtype = subtype };
        }
    }
}
