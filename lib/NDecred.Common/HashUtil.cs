using Org.BouncyCastle.Crypto.Digests;

namespace NDecred.Common
{
    public static class HashUtil
    {
        public static byte[] Ripemd160(byte[] data)
        {
            var ripemd = new RipeMD160Digest();
            ripemd.BlockUpdate(data, 0, data.Length);
            var output = new byte[20];
            ripemd.DoFinal(output, 0);
            return output;
        }

        public static byte[] Blake256(byte[] data)
        {
            using (var blake256 = new Blake256())
            {
                return blake256.ComputeHash(data);
            }
        }

        public static byte[] Blake256D(byte[] data)
        {
            return Blake256(Blake256(data));
        }
    }
}
