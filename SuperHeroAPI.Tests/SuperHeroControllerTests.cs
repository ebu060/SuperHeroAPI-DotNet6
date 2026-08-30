using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperHeroAPI.Data;
using SuperHeroAPI.Controllers;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace SuperHeroAPI.Tests
{
    public class SuperHeroControllerTests
    {
        [Fact]
        public async Task Get_ReturnsOnlyActiveHeroes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new DataContext(options))
            {
                // Add test data
                context.SuperHeroes.Add(new SuperHero { Id = 1, Name = "Hero1", IsActive = true });
                context.SuperHeroes.Add(new SuperHero { Id = 2, Name = "Hero2", IsActive = false });
                context.SuperHeroes.Add(new SuperHero { Id = 3, Name = "Hero3", IsActive = true });
                await context.SaveChangesAsync();

                var controller = new SuperHeroController(context);

                // Act
                var result = await controller.Get();

                // Assert
                var actionResult = Assert.IsType<OkObjectResult>(result.Result);
                var heroes = Assert.IsType<List<SuperHero>>(actionResult.Value);
                Assert.Equal(2, heroes.Count); // Only active heroes should be returned
                Assert.All(heroes, hero => Assert.True(hero.IsActive));
            }
        }

        [Fact]
        public async Task Delete_MarksHeroAsInactive()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new DataContext(options))
            {
                // Add test data
                context.SuperHeroes.Add(new SuperHero { Id = 1, Name = "Hero1", IsActive = true });
                await context.SaveChangesAsync();

                var controller = new SuperHeroController(context);

                // Act
                var result = await controller.Delete(1);

                // Assert
                var actionResult = Assert.IsType<OkObjectResult>(result.Result);
                var heroes = Assert.IsType<List<SuperHero>>(actionResult.Value);
                Assert.Single(heroes); // Only one hero should remain
                Assert.Equal(false, heroes[0].IsActive); // The hero should be inactive now
                Assert.NotNull(heroes[0].DeletedAt); // DeletedAt should not be null
            }
        }

        [Fact]
        public async Task GetArchived_ReturnsOnlyInactiveHeroes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new DataContext(options))
            {
                // Add test data
                context.SuperHeroes.Add(new SuperHero { Id = 1, Name = "Hero1", IsActive = true });
                context.SuperHeroes.Add(new SuperHero { Id = 2, Name = "Hero2", IsActive = false });
                context.SuperHeroes.Add(new SuperHero { Id = 3, Name = "Hero3", IsActive = false });
                await context.SaveChangesAsync();

                var controller = new SuperHeroController(context);

                // Act
                var result = await controller.GetArchived();

                // Assert
                var actionResult = Assert.IsType<OkObjectResult>(result.Result);
                var heroes = Assert.IsType<List<SuperHero>>(actionResult.Value);
                Assert.Equal(2, heroes.Count); // Only inactive heroes should be returned
                Assert.All(heroes, hero => Assert.False(hero.IsActive));
            }
        }
    }
}