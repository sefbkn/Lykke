using System.Threading.Tasks;

namespace Decred.Common.Client
{
    public interface IDcrdClient
    {        
        Task<DcrdRpcResponse<string>> PingAsync();
        Task<DcrdRpcResponse<string>> SendRawTransactionAsync(string hexTransaction);
        
        Task<GetBestBlockResult> GetBestBlockAsync();
        
        // Returns estimated fee as dcr/kb
        Task<decimal> EstimateFeeAsync(int numBlocks);
    }
    
    public class DcrdRpcResponse<T>
    {
        public string Id { get; set; }
        public string Jsonrpc { get; set; }
        public T Result { get; set; }
        public DcrdRpcError Error { get; set; }
        
        public class DcrdRpcError
        {
            public int? Code { get; set; }
            public string Message { get; set; }
        }
    }

    public class GetBestBlockResult
    {
        public string Hash { get; set; }
        public int Height { get; set; }
    }
}