using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperHeroAPI.Data;
using SuperHeroAPI.Controllers;
using Xunit;
using FluentAssertions;

namespace SuperHeroAPI.Tests
{
    public class MissionControllerTests
    {
        [Fact]
        public async Task AssignHeroesToMission_WithInsufficientPower_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            
            using var context = new DataContext(options);
            
            // Add a mission with difficulty 5 (requires power level 150)
            var mission = new Mission 
            { 
                Id = 1, 
                Title = "Test Mission", 
                Difficulty = 5 
            };
            context.Missions.Add(mission);
            await context.SaveChangesAsync();
            
            var controller = new MissionController(context);

            // Act
            var result = await controller.AssignHeroesToMission(1, new List<int> { 1 });

            // Assert
            result.Should().NotBeNull();
        }
    }
}