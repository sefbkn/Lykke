using System.Linq;
using System.Threading.Tasks;
using Decred.Common.Client;
using MoreLinq;
using Paymetheus.Decred;

namespace Lykke.Service.Decred.Api.Services
{
    public interface ITransactionFeeService
    {
        /// <summary>
        /// Returns the number of atoms.
        /// </summary>
        /// <returns></returns>
        Task<decimal> GetFeePerKb();
        long CalculateFee(decimal feePerKb, int numInputs, int numOutputs, decimal feeFactor);
    }
    
    public class TransactionFeeService : ITransactionFeeService
    {
        private const int AtomsPerDcr = 100000000;

        private readonly IDcrdClient _dcrdClient;
        private static readonly Transaction.Output[] _dummyOutput = { new Transaction.Output(-1, 0, new byte[25]) };

        public TransactionFeeService(IDcrdClient dcrdClient)
        {
            _dcrdClient = dcrdClient;
        }


        public async Task<decimal> GetFeePerKb()
        {
            const int numBlocks = 12 * 6;
            return await _dcrdClient.EstimateFeeAsync(numBlocks);
        }

        public long CalculateFee(decimal feePerKb, int numInputs, int numOutputs, decimal feeFactor)
        {
            var outputs = _dummyOutput.Repeat(numOutputs).ToArray();
            var serializeSizeBytes = Transaction.EstimateSerializeSize(numInputs, outputs, true);
            var serializeSizeKb = serializeSizeBytes / 1024m;
            return (long)(serializeSizeKb * feePerKb * feeFactor * AtomsPerDcr);
        }
    }
}
