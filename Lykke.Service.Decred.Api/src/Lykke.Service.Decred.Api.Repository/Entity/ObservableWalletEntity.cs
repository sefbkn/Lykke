using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Service.Decred.Api.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Decred.Api.Services
{
    /// <summary>
    /// Represents a wallet that may be watched for balance changes.
    /// </summary>
    public class ObservableWalletEntity : TableEntity
    {
        private string _address;

        public string Address
        {
            get { return _address; }
            set 
            { 
                _address = value;
                RowKey = value;
            }
        }

        public ObservableWalletEntity()
        {
            PartitionKey = "ByAddress";
            RowKey = Address;
        }
    }
}
