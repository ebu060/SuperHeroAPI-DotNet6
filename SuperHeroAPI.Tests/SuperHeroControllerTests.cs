using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SuperHeroAPI.Controllers;
using SuperHeroAPI.Data;
using SuperHeroAPI.Models;
using Xunit;

namespace SuperHeroAPI.Tests
{
    public class SuperHeroControllerTests
    {
        [Fact]
        public async Task Get_ReturnsOnlyActiveHeroes()
        {
            // Arrange
            var heroes = new List<SuperHero>
            {
                new SuperHero { Id = 1, Name = "Hero1", FirstName = "First1", LastName = "Last1", Place = "Place1", IsActive = true },
                new SuperHero { Id = 2, Name = "Hero2", FirstName = "First2", LastName = "Last2", Place = "Place2", IsActive = false },
                new SuperHero { Id = 3, Name = "Hero3", FirstName = "First3", LastName = "Last3", Place = "Place3", IsActive = true }
            };

            var mockContext = Substitute.For<DataContext>(new DbContextOptions<DataContext>());
            var mockSet = Substitute.For<DbSet<SuperHero>, IQueryable<SuperHero>>();
            
            // Setup the mock to return our test data
            ((IQueryable<SuperHero>)mockSet).Provider.Returns(heroes.AsQueryable().Provider);
            ((IQueryable<SuperHero>)mockSet).Expression.Returns(heroes.AsQueryable().Expression);
            ((IQueryable<SuperHero>)mockSet).ElementType.Returns(heroes.AsQueryable().ElementType);
            ((IQueryable<SuperHero>)mockSet).GetEnumerator().Returns(heroes.AsQueryable().GetEnumerator());

            mockContext.SuperHeroes.Returns(mockSet);

            var controller = new SuperHeroController(mockContext);

            // Act
            var result = await controller.Get();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedHeroes = Assert.IsType<List<SuperHero>>(actionResult.Value);
            returnedHeroes.Count.Should().Be(2);
            returnedHeroes.All(h => h.IsActive).Should().BeTrue();
        }

        [Fact]
        public async Task Delete_MarksHeroAsInactiveAndSetsDeletedAt()
        {
            // Arrange
            var hero = new SuperHero 
            { 
                Id = 1, 
                Name = "Hero1", 
                FirstName = "First1", 
                LastName = "Last1", 
                Place = "Place1", 
                IsActive = true,
                DeletedAt = null
            };

            var mockContext = Substitute.For<DataContext>(new DbContextOptions<DataContext>());
            var mockSet = Substitute.For<DbSet<SuperHero>, IQueryable<SuperHero>>();
            
            // Setup the mock to return our test data
            ((IQueryable<SuperHero>)mockSet).Provider.Returns(new List<SuperHero> { hero }.AsQueryable().Provider);
            ((IQueryable<SuperHero>)mockSet).Expression.Returns(new List<SuperHero> { hero }.AsQueryable().Expression);
            ((IQueryable<SuperHero>)mockSet).ElementType.Returns(new List<SuperHero> { hero }.AsQueryable().ElementType);
            ((IQueryable<SuperHero>)mockSet).GetEnumerator().Returns(new List<SuperHero> { hero }.AsQueryable().GetEnumerator());

            mockContext.SuperHeroes.Returns(mockSet);
            mockContext.SaveChangesAsync().Returns(Task.CompletedTask);

            var controller = new SuperHeroController(mockContext);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedHeroes = Assert.IsType<List<SuperHero>>(actionResult.Value);
            returnedHeroes.Count.Should().Be(1); // Only active heroes should be returned
            returnedHeroes.First().IsActive.Should().BeFalse();
            returnedHeroes.First().DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetArchived_ReturnsOnlyInactiveHeroes()
        {
            // Arrange
            var heroes = new List<SuperHero>
            {
                new SuperHero { Id = 1, Name = "Hero1", FirstName = "First1", LastName = "Last1", Place = "Place1", IsActive = true },
                new SuperHero { Id = 2, Name = "Hero2", FirstName = "First2", LastName = "Last2", Place = "Place2", IsActive = false, DeletedAt = DateTime.UtcNow },
                new SuperHero { Id = 3, Name = "Hero3", FirstName = "First3", LastName = "Last3", Place = "Place3", IsActive = false, DeletedAt = DateTime.UtcNow.AddDays(-1) }
            };

            var mockContext = Substitute.For<DataContext>(new DbContextOptions<DataContext>());
            var mockSet = Substitute.For<DbSet<SuperHero>, IQueryable<SuperHero>>();
            
            // Setup the mock to return our test data
            ((IQueryable<SuperHero>)mockSet).Provider.Returns(heroes.AsQueryable().Provider);
            ((IQueryable<SuperHero>)mockSet).Expression.Returns(heroes.AsQueryable().Expression);
            ((IQueryable<SuperHero>)mockSet).ElementType.Returns(heroes.AsQueryable().ElementType);
            ((IQueryable<SuperHero>)mockSet).GetEnumerator().Returns(heroes.AsQueryable().GetEnumerator());

            mockContext.SuperHeroes.Returns(mockSet);

            var controller = new SuperHeroController(mockContext);

            // Act
            var result = await controller.GetArchived();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedHeroes = Assert.IsType<List<SuperHero>>(actionResult.Value);
            returnedHeroes.Count.Should().Be(2);
            returnedHeroes.All(h => !h.IsActive).Should().BeTrue();
        }
    }
}