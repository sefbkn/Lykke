namespace Decred.Common
{
    public class Mainnet : Network
    {
        public override string Name => "Mainnet";
        public override AddressPrefix AddressPrefix => new MainnetAddressPrefix();
    }
    
    public class MainnetAddressPrefix : AddressPrefix
    {
        public override string NetworkAddressPrefix => "D";
        public override byte[] PayToPublicKeyHash => new byte[] {0x07, 0x3f};
        public override byte[] PrivateKey => new byte[] {0x22, 0xde};
    }
}
