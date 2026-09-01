using Events.IntegrationTests.Api.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;

namespace Events.IntegrationTests.Kafka;

public class KafkaApiWebApplicationFactory(string connectionString, string kafkaBootstrapServers)
    : ApiWebApplicationFactory(connectionString)
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        AddCommonTestServices(services);

        var kafkaSettingsDescriptor = services.Single(d => d.ServiceType == typeof(KafkaSettings));
        services.Remove(kafkaSettingsDescriptor);

        var kafkaSettings = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json"), optional: false)
            .Build()
            .GetSection(KafkaSettings.SectionName)
            .Get<KafkaSettings>()
            ?? throw new InvalidOperationException("Секция конфигурации 'Kafka' не найдена в appsettings.Testing.json.");

        kafkaSettings.BootstrapServers = kafkaBootstrapServers;
        services.AddSingleton(kafkaSettings);
    }
}
