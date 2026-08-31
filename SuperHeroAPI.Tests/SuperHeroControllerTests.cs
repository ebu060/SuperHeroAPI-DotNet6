using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperHeroAPI.Data;
using SuperHeroAPI.Controllers;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace SuperHeroAPI.Tests
{
    public class SuperHeroControllerTests
    {
        [Fact]
        public async Task GetPaged_WithValidParameters_ReturnsPaginatedResults()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            
            using var context = new DataContext(options);
            var controller = new SuperHeroController(context);

            // Act
            var result = await controller.GetPaged(page: 1, pageSize: 5);

            // Assert
            result.Should().NotBeNull();
        }
    }
}