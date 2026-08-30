using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;

public class Tests.ConsumerTests.ApiContractConsumerTests
{
    private readonly HttpClient _httpClient;

    public ApiContractConsumerTests()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:5001/");
    }

    [Fact]
    public async Task Get_Superheroes_Endpoint_Returns_Active_Heroes_Only()
    {
        // Act
        var response = await _httpClient.GetAsync("api/v1/superhero");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var heroes = JsonConvert.DeserializeObject<Hero[]>(content);
        
        Assert.NotNull(heroes);
        Assert.All(heroes, hero =>
        {
            Assert.True(hero.IsActive, "All returned heroes should be active");
        });
    }

    [Fact]
    public async Task Get_Archived_Superheroes_Endpoint_Returns_Inactive_Heroes()
    {
        // Act
        var response = await _httpClient.GetAsync("api/v1/superhero/archived");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var heroes = JsonConvert.DeserializeObject<Hero[]>(content);
        
        Assert.NotNull(heroes);
        Assert.All(heroes, hero =>
        {
            Assert.False(hero.IsActive, "All returned heroes should be inactive");
        });
    }

    [Fact]
    public async Task Delete_Superhero_Endpoint_Soft_Deletes_Hero()
    {
        // Arrange
        var heroId = 1;
        
        // Act
        var response = await _httpClient.DeleteAsync($"api/v1/superhero/{heroId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify the hero is marked as inactive
        var getResponse = await _httpClient.GetAsync($"api/v1/superhero/{heroId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var content = await getResponse.Content.ReadAsStringAsync();
        var hero = JsonConvert.DeserializeObject<Hero>(content);
        
        Assert.False(hero.IsActive, "Hero should be marked as inactive after soft delete");
        Assert.NotNull(hero.DeletedAt, "DeletedAt should be set after soft delete");
    }

    [Fact]
    public async Task Get_Superhero_Endpoint_Returns_Hero_With_All_Required_Fields()
    {
        // Act
        var response = await _httpClient.GetAsync("api/v1/superhero");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var heroes = JsonConvert.DeserializeObject<Hero[]>(content);
        
        Assert.NotNull(heroes);
        Assert.NotEmpty(heroes);
        
        var firstHero = heroes[0];
        Assert.True(firstHero.Id > 0, "Hero ID should be greater than 0");
        Assert.NotNull(firstHero.Name, "Hero name should not be null");
        Assert.NotNull(firstHero.Power, "Hero power should not be null");
        Assert.NotNull(firstHero.CreatedAt, "Hero created at date should not be null");
        Assert.True(firstHero.IsActive, "Hero should be active by default");
    }

    [Fact]
    public async Task Get_Archived_Superhero_Endpoint_Returns_Hero_With_All_Required_Fields()
    {
        // Act
        var response = await _httpClient.GetAsync("api/v1/superhero/archived");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var heroes = JsonConvert.DeserializeObject<Hero[]>(content);
        
        Assert.NotNull(heroes);
        
        if (heroes.Length > 0)
        {
            var firstHero = heroes[0];
            Assert.True(firstHero.Id > 0, "Hero ID should be greater than 0");
            Assert.NotNull(firstHero.Name, "Hero name should not be null");
            Assert.NotNull(firstHero.Power, "Hero power should not be null");
            Assert.NotNull(firstHero.CreatedAt, "Hero created at date should not be null");
            Assert.False(firstHero.IsActive, "Hero should be inactive in archive");
        }
    }

    public class Hero
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Power { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}