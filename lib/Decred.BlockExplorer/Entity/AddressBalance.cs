namespace Decred.BlockExplorer
{
    public class AddressBalance
    {
        public string Address { get; set; }
        public long Balance { get; set; }
        public long Block { get; set; }

        public AddressBalance()
        {
        }
    }
}