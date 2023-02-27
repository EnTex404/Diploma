using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public static class AttributeProcessig
    {
        private static List<Tuple<FieldInfo, byte>> GetFields(Type t)
        {
            return t.GetFields(BindingFlags.Instance |
                               BindingFlags.NonPublic |
                               BindingFlags.Public)
                .Where(field => field.GetCustomAttribute<FieldAttribute>() != null)
                .Select(field => Tuple.Create(field, field.GetCustomAttribute<FieldAttribute>().FieldId))
                .ToList();
        }

        public static Package Serialize(byte type, byte subtype, object obj, bool strict = false)
        {
            var fields = GetFields(obj.GetType());

            if (strict)
            {
                var usedUp = new List<byte>();

                foreach (var field in fields)
                {
                    if (usedUp.Contains(field.Item2))
                    {
                        throw new Exception("One field used two times.");
                    }

                    usedUp.Add(field.Item2);
                }
            }

            var packet = Package.Create(type, subtype);

            foreach (var field in fields)
            {
                packet.SetValue(field.Item2, field.Item1.GetValue(obj));
            }

            return packet;
        }

        public static Package Serialize(PackageType type, object obj, bool strict = false)
        {
            var types = PackageTypeManager.GetType(type);
            return Serialize(types.Item1, types.Item2, obj, strict);
        }

        public static T Deserialize<T>(Package package, bool strict = false)
        {
            var fields = GetFields(typeof(T));
            var instance = Activator.CreateInstance<T>();

            if (fields.Count == 0)
            {
                return instance;
            }

            foreach (var tuple in fields)
            {
                var field = tuple.Item1;
                var packageFieldId = tuple.Item2;

                if (!package.HasField(packageFieldId))
                {
                    if (strict)
                    {
                        throw new Exception($"Couldn't get field[{packageFieldId}] for {field.Name}");
                    }

                    continue;
                }

                var value = typeof(Package)
                    .GetMethod("GetValue")?
                    .MakeGenericMethod(field.FieldType)
                    .Invoke(package, new object[] { packageFieldId });

                if (value == null)
                {
                    if (strict)
                    {
                        throw new Exception($"Couldn't get value for field[{packageFieldId}] for {field.Name}");
                    }

                    continue;
                }

                field.SetValue(instance, value);
            }

            return instance;
        }
    }
}
