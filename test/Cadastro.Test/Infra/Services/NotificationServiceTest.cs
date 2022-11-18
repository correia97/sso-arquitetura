using Cadastro.Data.Services;
using Cadastro.Domain.Entities;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace Cadastro.Test.Infra.Services
{
    public class NotificationServiceTest
    {
        private Mock<IModel> _mockModel;
        private Mock<IBasicProperties> _mockBasicProperties;

        public readonly Faker _faker;
        private ITestOutputHelper Output { get; }
        public NotificationServiceTest(ITestOutputHelper outputHelper)
        {

            _mockModel = new Mock<IModel>();
            _mockBasicProperties = new Mock<IBasicProperties>();
            _faker = new Faker("pt_BR");
            Output = outputHelper;
        }

        [Fact]
        public void SendNotification_Test()
        {
            _mockModel.Setup(x => x.CreateBasicProperties())
                .Returns(_mockBasicProperties.Object);

            var notification = new NotificationMessage(Guid.NewGuid(), Guid.NewGuid(), _faker.Random.String(), true);

            var appService = new NotificationService(_mockModel.Object);

            appService.SendEvent(notification);

            _mockModel.Verify(x => x.CreateBasicProperties(), Times.Once);
        }
    }
}
