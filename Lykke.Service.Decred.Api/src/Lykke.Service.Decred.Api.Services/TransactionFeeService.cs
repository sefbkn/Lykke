using System.Linq;
using MoreLinq;
using Paymetheus.Decred;

namespace Lykke.Service.Decred.Api.Services
{
    public interface ITransactionFeeService
    {
        long CalculateFee(int numInputs, int numOutputs, decimal feeFactor);
    }
    
    public class TransactionFeeService : ITransactionFeeService
    {
        private static readonly Transaction.Output[] _dummyOutput = { new Transaction.Output(-1, 0, new byte[25]) };

        public long CalculateFee(int numInputs, int numOutputs, decimal feeFactor)
        {
            var outputs = _dummyOutput.Repeat(numOutputs).ToArray();
            var dynamicFee = Transaction.EstimateSerializeSize(numInputs, outputs, true) * feeFactor;
            var flatFee = (long)(0.001m * 100000000 * feeFactor);
            return flatFee;
        }
    }
}
