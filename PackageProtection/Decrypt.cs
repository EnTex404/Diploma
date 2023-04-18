using Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace PackageProtection
{
    public partial class Encryption
    {
        public Package DecryptPackage(Package package)
        {

            if (!package.HasField(0))
            {
                return null;
            }
            
            Aes aes = Aes.Create();

            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            var value = package.GetValueRaw(0);

            using(var stream = new MemoryStream(value))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(LenK, 0, 3);
                stream.Seek(4, SeekOrigin.Begin);
                stream.Read(LenIV, 0, 3);

                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                int startC = lenK + lenIV + 8;
                int lenC = (int)stream.Length - startC;

                byte[] KeyEncrypted = new byte[lenK];
                byte[] IV = new byte[lenIV];

                stream.Seek(8, SeekOrigin.Begin);
                stream.Read(KeyEncrypted, 0, lenK);
                stream.Seek(8 + lenK, SeekOrigin.Begin);
                stream.Read(IV, 0, lenIV);

                byte[] KeyDecrypted = Configuration._rsa.Decrypt(KeyEncrypted, false);

                ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV);

                using(var decrStream = new MemoryStream())
                {
                    int count = 0;
                    int offset = 0;

                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];

                    decrStream.Seek(startC, SeekOrigin.Begin);
                    using(var outStreamDecrypted = new CryptoStream(decrStream, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = stream.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamDecrypted.Write(data, 0, count);
                        } while (count > 0);

                        outStreamDecrypted.FlushFinalBlock();
                    }

                    var list = new List<byte>(decrStream.ToArray());

                    while (list[0] == 0)
                    {
                        list.RemoveAt(0);
                    }

                    var outPackage = Package.Parse(list.ToArray());

                    return outPackage;
                }

            }
        }
    }
}
