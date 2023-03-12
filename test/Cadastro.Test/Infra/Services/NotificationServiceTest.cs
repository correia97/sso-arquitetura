using Cadastro.Data.Services;
using Cadastro.Domain.Entities;
using Cadastro.Domain.Enums;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace Cadastro.Test.Infra.Services
{
    public class NotificationServiceTest
    {
        private readonly Mock<IModel> _mockModel;
        private readonly Mock<IBasicProperties> _mockBasicProperties;

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

            var notification = new NotificationMessage<string>(Guid.NewGuid(), Guid.NewGuid(), TipoEvento.None, true, _faker.Random.String());

            var appService = new NotificationService(_mockModel.Object);

            appService.SendEvent(notification);

            _mockModel.Verify(x => x.CreateBasicProperties(), Times.Once);

            Output.WriteLine(notification.ToString());
        }
    }
}
