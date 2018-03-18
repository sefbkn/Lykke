﻿using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.Decred.Api.Common;
using Lykke.Service.Decred.Api.Common.Entity;
using Lykke.Service.Decred.Api.Repository;
using Moq;
using Xunit;

namespace Lykke.Service.Decred.Api.Services.Test
{
    public class BalanceServiceTests
    {
        private static Mock<IBlockRepository> _mockBlockRepo;
        private static Mock<IAddressValidationService> _mockAddressValidator;
        private static Mock<IAddressRepository> _addressBalanceRepository;
        private static Mock<INosqlRepo<ObservableWalletEntity>> _mockOperationRepo;
        
        public BalanceServiceTests()
        {
            _mockBlockRepo = new Mock<IBlockRepository>();
            _mockAddressValidator = new Mock<IAddressValidationService>();
            _addressBalanceRepository = new Mock<IAddressRepository>();
            _mockOperationRepo = new Mock<INosqlRepo<ObservableWalletEntity>>();
        }
        
        [Fact]
        public async Task GetDataWithContinuationTokenAsync_CallsCorrectRepositoryMethod()
        {
            var subject = new BalanceService(
                _mockOperationRepo.Object, 
                _addressBalanceRepository.Object, 
                _mockBlockRepo.Object,
                _mockAddressValidator.Object);
            
            _mockOperationRepo.Setup(x => x.GetDataWithContinuationTokenAsync(1, "test"))
                .ReturnsAsync((new[]
                {
                    new ObservableWalletEntity{ Address = "address" },
                }, "test2"));

            _mockAddressValidator.Setup(m => m.IsValid(It.IsAny<string>())).Returns(true);
            
            var result = await subject.GetBalancesAsync(1, "test");
            
            Assert.Equal(1, result.Items.Count);
            Assert.Equal("address", (string) result.Items[0].Address);
        }
    }
}
