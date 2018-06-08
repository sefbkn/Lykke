using System.Threading.Tasks;

namespace DcrdClient
{
    public interface IDcrdClient
    {        
        Task<DcrdRpcResponse<string>> PingAsync();
        Task<DcrdRpcResponse<string>> SendRawTransactionAsync(string hexTransaction);
        
        Task<GetBestBlockResult> GetBestBlockAsync();
        Task<long> GetMaxConfirmedBlockHeight();
        
        // Returns estimated fee as dcr/kb
        Task<decimal> EstimateFeeAsync(int numBlocks);
    }
}