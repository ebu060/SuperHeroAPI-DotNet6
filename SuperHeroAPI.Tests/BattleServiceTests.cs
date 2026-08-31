using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SuperHeroAPI.Data;
using System.Threading.Tasks;
using Xunit;

namespace SuperHeroAPI.Tests
{
    public class BattleServiceTests
    {
        [Fact]
        public async Task SimulateBattle_WhenAttackerAndDefenderAreSame_ShouldReturnBadRequest()
        {
            // Arrange
            var context = new DataContext(new DbContextOptionsBuilder<DataContext>().UseInMemoryDatabase("TestDb").Options);
            
            // Act
            var controller = new BattleController(context);
            var request = new BattleController.BattleRequest
            {
                AttackerHeroId = 1,
                DefenderHeroId = 1
            };
            
            var result = await controller.SimulateBattle(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SimulateBattle_WhenOneHeroIsBusy_ShouldReturnConflict()
        {
            // Arrange
            var context = new DataContext(new DbContextOptionsBuilder<DataContext>().UseInMemoryDatabase("TestDb").Options);
            
            // Add test heroes
            var attacker = new SuperHero { Id = 1, Name = "Attacker", IsBusy = false };
            var defender = new SuperHero { Id = 2, Name = "Defender", IsBusy = true };
            
            context.SuperHeroes.Add(attacker);
            context.SuperHeroes.Add(defender);
            await context.SaveChangesAsync();

            // Act
            var controller = new BattleController(context);
            var request = new BattleController.BattleRequest
            {
                AttackerHeroId = 1,
                DefenderHeroId = 2
            };
            
            var result = await controller.SimulateBattle(request);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
        }
    }
}