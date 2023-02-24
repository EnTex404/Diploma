using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    internal class Package
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

        public byte[] ToPackage()
        {
            var packet = new MemoryStream();

            packet.Write(ChangeHeaders
                ? new byte[] { 0x95, 0xAA, 0xFF, PackageType, PackageSubtype } :
                  new byte[] { 0xAF, 0xAA, 0xAF, PackageType, PackageSubtype }, 0, 5);

            var fields = Fields.OrderBy(f => f.FieldID);

            foreach (var field in fields)
            {
                packet.Write(new[] { field.FieldID, field.FieldSize }, 0, 2);
                packet.Write(field.Contents, 0, field.Contents.Length);
            }

            packet.Write(new byte[] { 0xFF, 0x00 }, 0, 2);

            return packet.ToArray();
        }

    }
}
