using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api.Repository
{
    public enum RecordType
    {
        UnsignedTransaction,
        BroadcastedTransaction,
        ObservedAddressOutgoing,
        ObservedAddressIncoming
    }
    
    public class KeyValueEntity : TableEntity
    {
        private readonly RecordType _recordType;
        public string Value { get; set; }
        
        public KeyValueEntity() { }
        public KeyValueEntity(RecordType recordType, string key, string value)
        {
            _recordType = recordType;
            RowKey = GetRowKey(recordType, key);
            Value = value;
            PartitionKey = "ByRowKey";
        }

        public static string GetRowKey(RecordType recordType, string key)
        {
            return $"{recordType}.{key}";
        }
    }
}
