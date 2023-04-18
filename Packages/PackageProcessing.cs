using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public partial class Package
    {
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

        public static Package Parse(byte[] packet)
        {
            if (packet.Length < 7)
            {
                return null;
            }

            var maxIndex = packet.Length - 1;

            if (packet[maxIndex - 1] != 0xFF ||
                packet[maxIndex] != 0x00)
            {
                return null;
            }

            var type = packet[3];
            var subtype = packet[4];

            var newPackage = Create(type, subtype);

            var fields = packet.Skip(5).ToArray();

            while (true)
            {
                if (fields.Length == 2)
                {
                    return newPackage;
                }

                var id = fields[0];
                var size = fields[1];

                var contents = size != 0 ? fields.Skip(2).Take(size).ToArray() : null;

                newPackage.Fields.Add(new PackageField()
                {
                    FieldID = id,
                    FieldSize = size,
                    Contents = contents
                });

                fields = fields.Skip(2 + size).ToArray();
            }
        }

        public byte[] FixedObjectToByteArray(object value)
        {
            var rawsize = Marshal.SizeOf(value);
            var rawdata = new byte[rawsize];

            var handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);

            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);

            handle.Free();

            return rawdata;
        }

        private T ByteArrayToFixedObject<T>(byte[] bytes)
        {
            T structure;

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return structure;
        }
    }
}
