using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Decred.Api.Repository
{
    public class ObservableTransaction : TableEntity
    {
        public const string Key = "ByOperationId";

        private Guid _operationId;
        public Guid OperationId
        {
            get { return _operationId; }
            set
            {
                _operationId = value;
                RowKey = value.ToString();
            }
        }

        public string TransactionHash { get; set; }
        public string FromAddress { get; set; }
        public string FromAddressContext { get; set; }
        public string ToAddress { get; set; }
        public string AssetId { get; set; }
        public string Amount { get; set; }
        public bool IncludeFee { get; set; }

        public ObservableTransaction()
        {
            PartitionKey = Key;
            RowKey = OperationId.ToString();
        }
    }
}
