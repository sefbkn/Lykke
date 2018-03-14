using System.Collections.Generic;

namespace NDecred.Common
{
    public abstract class AddressPrefix
    {
        public IEnumerable<byte[]> All => new[]
        {
            PayToPublicKeyHash, PrivateKey
        };

        public abstract string NetworkAddressPrefix { get; }
        public abstract byte[] PayToPublicKeyHash { get; }
        public abstract byte[] PrivateKey { get; }
    }
}
