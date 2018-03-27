using System.Threading.Tasks;

namespace DcrdClient
{
    public interface IDcrdClient
    {        
        Task<DcrdRpcResponse<string>> PingAsync();
        Task<DcrdRpcResponse<string>> SendRawTransactionAsync(string hexTransaction);
        
        Task<GetBestBlockResult> GetBestBlockAsync();
        
        // Returns estimated fee as dcr/kb
        Task<decimal> EstimateFeeAsync(int numBlocks);
    }
}