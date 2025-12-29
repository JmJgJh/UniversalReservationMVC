using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Tests.Fakes;

namespace UniversalReservationMVC.Tests;

public class ResourceServiceTests
{
    [Fact]
    public async Task CreateResource_CreatesResourceSuccessfully()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<ResourceService>>();
        var service = new ResourceService(unitOfWork, logger);

        var newResource = new Resource
        {
            Name = "Conference Room A",
            Description = "Large conference room",
            ResourceType = ResourceType.ConferenceRoom,
            CompanyId = 1,
            Price = 100.00m,
            Capacity = 50
        };

        // Act
        var result = await service.CreateResourceAsync(newResource);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Conference Room A", result.Name);
        Assert.Equal(ResourceType.ConferenceRoom, result.ResourceType);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task GetResourcesByCompany_ReturnsOnlyCompanyResources()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<ResourceService>>();
        var service = new ResourceService(unitOfWork, logger);

        var resource1 = new Resource
        {
            Name = "Resource 1",
            ResourceType = ResourceType.Cinema,
            CompanyId = 1,
            Price = 50m
        };
        var resource2 = new Resource
        {
            Name = "Resource 2",
            ResourceType = ResourceType.Restaurant,
            CompanyId = 2,
            Price = 75m
        };

        await unitOfWork.Resources.AddAsync(resource1);
        await unitOfWork.Resources.AddAsync(resource2);
        await unitOfWork.SaveAsync();

        // Act
        var result = await service.GetResourcesByCompanyAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Resource 1", result.First().Name);
    }

    [Fact]
    public async Task UpdateResource_UpdatesResourceSuccessfully()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<ResourceService>>();
        var service = new ResourceService(unitOfWork, logger);

        var resource = new Resource
        {
            Name = "Original Name",
            ResourceType = ResourceType.Office,
            CompanyId = 1,
            Price = 50m
        };
        await unitOfWork.Resources.AddAsync(resource);
        await unitOfWork.SaveAsync();

        // Act
        resource.Name = "Updated Name";
        resource.Price = 75m;
        await service.UpdateResourceAsync(resource);

        // Assert
        var updated = await unitOfWork.Resources.GetByIdAsync(resource.Id);
        Assert.Equal("Updated Name", updated?.Name);
        Assert.Equal(75m, updated?.Price);
    }

    [Fact]
    public async Task DeleteResource_RemovesResource()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<ResourceService>>();
        var service = new ResourceService(unitOfWork, logger);

        var resource = new Resource
        {
            Name = "To Delete",
            ResourceType = ResourceType.Theatre,
            CompanyId = 1,
            Price = 50m
        };
        await unitOfWork.Resources.AddAsync(resource);
        await unitOfWork.SaveAsync();

        // Act
        await service.DeleteResourceAsync(resource.Id);

        // Assert
        var deleted = await unitOfWork.Resources.GetByIdAsync(resource.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetResourceById_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<ResourceService>>();
        var service = new ResourceService(unitOfWork, logger);

        // Act
        var result = await service.GetResourceByIdAsync(999);

        // Assert
        Assert.Null(result);
    }
}
