using System;
using System.Threading.Tasks;
using Pacco.Services.Availability.Api;
using Pacco.Services.Availability.Application.Commands;
using Pacco.Services.Availability.Application.Events;
using Pacco.Services.Availability.Infrastructure.Mongo.Documents;
using Pacco.Services.Availability.Tests.Shared.Factories;
using Pacco.Services.Availability.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace Pacco.Services.Availability.Tests.Integration.Async
{
    public class AddResourceTests : IDisposable, IClassFixture<PaccoApplicationFactory<Program>>
    {
        private Task Act(AddResource command) => _rabbitMqFixture.PublishAsync(command, Exchange);
        
        [Fact]
        public async Task AddResource_Endpoint_Should_Add_Resource_With_Given_Id_To_Database()
        {
            var command = new AddResource(_resourceId, _tags);

            await Act(command);
            
            var tcs = _rabbitMqFixture.SubscribeAndGet<ResourceAdded, ResourceDocument>(Exchange,
                _mongoDbFixture.GetAsync, command.ResourceId);
            
            var document = await tcs.Task;
            
            document.ShouldNotBeNull();
            document.Id.ShouldBe(command.ResourceId);
            document.Tags.ShouldBe(_tags);
        }
        
        #region ARRANGE    
        
        private readonly MongoDbFixture<ResourceDocument, Guid> _mongoDbFixture;
        private readonly RabbitMqFixture _rabbitMqFixture;
        private readonly Guid _resourceId;
        private readonly string[] _tags;
        private const string Exchange = "availability";

        public AddResourceTests(PaccoApplicationFactory<Program> factory)
        {
            _resourceId = Guid.Parse("587acaf9-629f-4896-a893-4e94ae628652");
            _tags = new[]{"tags"};
            _rabbitMqFixture = new RabbitMqFixture("availability");
            _mongoDbFixture = new MongoDbFixture<ResourceDocument, Guid>("Resources");
            factory.Server.AllowSynchronousIO = true;
        }
        
        public void Dispose()
        {
            _mongoDbFixture.Dispose();
        }
        
        #endregion
    }
}