using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public partial class Package
    {
        public PackageField GetField(byte id)
        {
            foreach (var field in Fields)
            {
                if (field.FieldID == id)
                {
                    return field;
                }
            }

            return null;
        }

        public bool HasField(byte id)
        {
            return GetField(id) != null;
        }

        public T GetValue<T>(byte id)
        {
            var field = GetField(id);

            CheckFieldFound(id, field);

            int neededSize;

            if (typeof(T).IsValueType)
            {
                neededSize = Marshal.SizeOf(typeof(T));
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    ms.WriteAsync(field.Contents, 0, field.Contents.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    var obj = bf.Deserialize(ms);
                    return (T)obj;
                }
            }

            CheckFieldSize<T>(field, neededSize);

            return ByteArrayToFixedObject<T>(field.Contents);
        }

        public void SetValue(byte id, object structure)
        {
            if (structure == null)
            {
                return;
            }

            byte[] bytes;


            if (!structure.GetType().IsValueType)
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, structure);
                    bytes = ms.ToArray();
                }
            }
            else
            {
                bytes = FixedObjectToByteArray(structure);
            }

            SetValue(id, bytes);
        }

        private void SetValue(byte id, byte[] rawData)
        {
            var field = GetField(id);

            field = CheckField(id, field);

            SetFieldValues(rawData, field);
        }

        public void SetValueRaw(byte id, byte[] data)
        {
            SetValue(id, data);
        }

        public byte[] GetValueRaw(byte id)
        {
            var field = GetField(id);

            CheckFieldFound(id, field);

            return field.Contents;
        }

        private static void SetFieldValues(byte[] bytes, PackageField field)
        {
            field.FieldSize = (byte)bytes.Length;
            field.Contents = bytes;
        }

        private PackageField CheckField(byte id, PackageField field)
        {
            if (field == null)
            {
                field = new PackageField()
                {
                    FieldID = id
                };

                Fields.Add(field);
            }

            return field;
        }

        private static void CheckFieldSize<T>(PackageField field, int neededSize)
        {
            if (field.FieldSize != neededSize)
            {
                throw new Exception($"Can't convert field to type {typeof(T).FullName}.\n" + $"We have {field.FieldSize} bytes but we need exactly {neededSize}.");
            }
        }

        private static void CheckFieldFound(byte id, PackageField field)
        {
            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }
        }

    }
}
