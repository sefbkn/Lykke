namespace NDecred.Common
{
    public class TestNet : Network
    {
        public override string Name => "testnet";
        public override AddressPrefix AddressPrefix => new TestnetAddressPrefix();
    }
    
    public class TestnetAddressPrefix : AddressPrefix
    {
        public override string NetworkAddressPrefix => "T";
        public override byte[] PayToPublicKeyHash => new byte[] {0x0f, 0x21};
        public override byte[] PrivateKey => new byte[] {0x23, 0x0e};
    }
}
