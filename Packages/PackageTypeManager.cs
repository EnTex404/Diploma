using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages
{
    public class PackageTypeManager
    {
        private static readonly Dictionary<PackageType, Tuple<byte, byte>> TypeDictionary = new Dictionary<PackageType, Tuple<byte, byte>>();

        public static void RegisterType(PackageType type, byte btype, byte bsubtype)
        {
            if (TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is already registered.");
            }

            TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
        }

        public static Tuple<byte, byte> GetType(PackageType type)
        {
            if (!TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is not registered.");
            }

            return TypeDictionary[type];
        }

        public static PackageType GetTypeFromPacket(Package packet)
        {
            if (packet == null)
            {
                return PackageType.UNKNOWN;
            }

            var type = packet.PackageType;
            var subtype = packet.PackageSubtype;

            foreach (var tuple in TypeDictionary)
            {
                var value = tuple.Value;

                if (value.Item1 == type && value.Item2 == subtype)
                {
                    return tuple.Key;
                }
            }

            return PackageType.UNKNOWN;
        }
    }
}
