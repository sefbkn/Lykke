using System.Threading.Tasks;

namespace Decred.Common.Client
{
    public interface IDcrdClient
    {
        Task<DcrdRpcResponse> SendRawTransactionAsync(string hexTransaction);
    }
    
    public class DcrdRpcResponse
    {
        public string Id { get; set; }
        public string Jsonrpc { get; set; }
        public string Result { get; set; }
        public DcrdRpcError Error { get; set; }
        
        public class DcrdRpcError
        {
            public int? Code { get; set; }
            public string Message { get; set; }
        }
    }
}