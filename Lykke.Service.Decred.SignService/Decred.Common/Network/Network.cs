using System;

namespace Decred.Common
{
    public abstract class Network
    {
        public static Network Mainnet => new Mainnet();
        public static Network Testnet => new TestNet();

        public static Network ByName(string networkName)
        {
            switch (networkName.ToLower())
            {
                case "mainnet": return Mainnet;
                case "testnet": return Testnet;
                default: 
                    throw new InvalidOperationException($"Attempted to create unknown network instance {networkName}");
            }
        }
        
        public abstract string Name { get; }
        public abstract AddressPrefix AddressPrefix { get; }
    }
}
