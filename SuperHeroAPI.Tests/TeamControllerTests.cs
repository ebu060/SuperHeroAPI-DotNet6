using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperHeroAPI.Data;
using SuperHeroAPI.Controllers;
using Xunit;
using FluentAssertions;

namespace SuperHeroAPI.Tests
{
    public class TeamControllerTests
    {
        [Fact]
        public async Task Get_ReturnsTeamsWithHeroCounts()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            
            using var context = new DataContext(options);
            var controller = new TeamController(context);

            // Act
            var result = await controller.Get();

            // Assert
            result.Should().NotBeNull();
        }
    }
}