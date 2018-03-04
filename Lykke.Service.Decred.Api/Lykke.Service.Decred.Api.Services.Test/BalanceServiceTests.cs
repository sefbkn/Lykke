using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.Decred.Api.Repository;
using Moq;
using Xunit;

namespace Lykke.Service.Decred.Api.Services.Test
{
    public class BalanceServiceTests
    {
        private static Mock<IBlockRepository> _mockBlockRepo;
        private static Mock<IAddressBalanceRepository> _addressBalanceRepository;
        private static Mock<IObservableOperationRepository<ObservableWalletEntity>> _mockOperationRepo;
        
        public BalanceServiceTests()
        {
            _mockBlockRepo = new Mock<IBlockRepository>();
            _addressBalanceRepository = new Mock<IAddressBalanceRepository>();
            _mockOperationRepo = new Mock<IObservableOperationRepository<ObservableWalletEntity>>();
        }
        
        [Fact]
        public async Task GetDataWithContinuationTokenAsync_CallsCorrectRepositoryMethod()
        {
            var subject = new BalanceService(
                _mockOperationRepo.Object, 
                _addressBalanceRepository.Object, 
                _mockBlockRepo.Object);
            
            _mockOperationRepo.Expect(x => x.GetDataWithContinuationTokenAsync(1, "test"))
                .ReturnsAsync((new[]
                {
                    new ObservableWalletEntity{ Address = "address" },
                }, "test2"));
            
            var result = await subject.GetBalancesAsync(1, "test");
            
            Assert.Equal(1, result.Items.Count);
            Assert.Equal("address", result.Items[0].Address);
        }
    }
}
