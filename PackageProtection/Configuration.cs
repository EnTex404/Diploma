using System.Security.Cryptography;

namespace PackageProtection
{
    public static class Configuration
    {
        public static readonly CspParameters _cspp = new();
        public static RSACryptoServiceProvider _rsa;

        public const string EncrFolder = @"\Encrypt\";
        public const string DecrFolder = @"\Decrypt\";
        public const string SrcFolder = @"\docs\";

        public const string PubKeyFile = @"\encrypt\rsaPublicKey.txt";

        public const string KeyName = "Key";
    }
}