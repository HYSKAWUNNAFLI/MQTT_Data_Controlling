using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using mqttpetproject.Application.Abstractions.Persistence;
using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Application.Services;
using mqttpetproject.Application.Validation;
using mqttpetproject.Application.Validation.Rules;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Enums;
using mqttpetproject.Tests.Support;

namespace mqttpetproject.Tests.Services;

public sealed class TelemetryProcessingServiceTests
{
    private readonly Mock<ITelemetryRepository> _repository = new();
    private readonly ITelemetryValidator _validator;

    public TelemetryProcessingServiceTests()
    {
        _validator = new TelemetryValidator(new ITopicRuleValidator[]
        {
            new ElectricityTopicRuleValidator(),
            new SteamTopicRuleValidator(),
            new GasTopicRuleValidator()
        });
    }

    [Fact]
    public async Task ProcessAsync_WhenNoSaveIsTrue_ShouldAckWithoutSaving()
    {
        var service = CreateService();
        var payload = TelemetrySamples.ValidElectricityJson(noSave: true);

        var result = await service.ProcessAsync(payload);

        result.Status.Should().Be(MessageProcessingStatus.Ack);
        result.SavedToDatabase.Should().BeFalse();
        _repository.Verify(
            repository => repository.SaveTelemetryAsync(It.IsAny<TelemetryMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenForceDbErrorIsTrue_ShouldNackRequeue()
    {
        var service = CreateService();
        var payload = TelemetrySamples.ValidElectricityJson(forceDbError: true);

        var result = await service.ProcessAsync(payload);

        result.Status.Should().Be(MessageProcessingStatus.NackRequeue);
        _repository.Verify(
            repository => repository.SaveTelemetryAsync(It.IsAny<TelemetryMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private TelemetryProcessingService CreateService()
    {
        return new TelemetryProcessingService(
            _validator,
            _repository.Object,
            new IdGenerator(),
            NullLogger<TelemetryProcessingService>.Instance);
    }
}
