using Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using static PackageProtection.Delegates;

namespace PackageProtection
{
    public partial class Encryption
    {
        private MessageDel? message;

        public void RegisterMessageDel(MessageDel del)
        {
            message += del;
        }

        public void UnregisterMessageDel(MessageDel del)
        {
            try
            {
                message -= del;
            }
            catch
            {
                DelegatesProcess.errorMessage?.Invoke("error");
            }
        }

        public void CreateASMKeys()
        {
            Configuration._cspp.KeyContainerName = Configuration.KeyName;

            Configuration._rsa = new RSACryptoServiceProvider(Configuration._cspp)
            {
                PersistKeyInCsp = true
            };

            if (Configuration._rsa.PublicOnly)
            {
                message?.Invoke($"Ключ: {Configuration._cspp.KeyContainerName} - Только общедоступный");
            }
            else
            {
                message?.Invoke($"Ключ: {Configuration._cspp.KeyContainerName} - Полная пара ключей");
            }
        }

        public Package EncryptPackage(Package package)
        {
            if (Configuration._rsa is null)
            {
                CreateASMKeys();
            }
            Aes aes = Aes.Create();
            ICryptoTransform transform = aes.CreateEncryptor();

            byte[] keyEncrypted = Configuration._rsa.Encrypt(aes.Key, false);

            int lKey = keyEncrypted.Length;
            byte[] LenK = BitConverter.GetBytes(lKey);
            int lIV = aes.IV.Length;
            byte[] LenIV = BitConverter.GetBytes(lIV);

            var stream = new MemoryStream();

            stream.Write(LenK, 0, 4);
            stream.Write(LenIV, 0, 4);
            stream.Write(keyEncrypted, 0, lKey);
            stream.Write(aes.IV, 0, lIV);

            using (var outStreamEncrypted = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                int count = 0;
                int offset = 0;

                int blockSizeBytes = aes.BlockSize / 8;
                byte[] data = new byte[blockSizeBytes];
                int bytesRead = 0;

                byte[] bytes = package.ToPackage();

                using (var ms = new MemoryStream(bytes))
                {
                    do
                    {
                        count = ms.Read(data, 0, blockSizeBytes);
                        offset += count;
                        outStreamEncrypted.Write(data, 0, count);
                        bytesRead += blockSizeBytes;

                    } while (count > 0);
                }
                outStreamEncrypted.FlushFinalBlock();

                var outPackage = Package.Create(PackageType.PROTECTED);
                outPackage.SetValueRaw(0, stream.ToArray());
                outPackage.ChangeHeaders = true;

                return outPackage;
            }
        }
    }
}
