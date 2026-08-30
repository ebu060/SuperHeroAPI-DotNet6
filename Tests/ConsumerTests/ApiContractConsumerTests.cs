using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

public class Tests.ConsumerTests.ApiContractConsumerTests
{
    private readonly HttpClient _httpClient;
    
    public ApiContractConsumerTests()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:5001/");
    }

    [Fact]
    public async Task GetSuperheroes_Should_Return_Active_Superheroes_Only()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/superheroes");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var superheroes = JsonConvert.DeserializeObject<List<Superhero>>(content);
        
        Assert.NotNull(superheroes);
        Assert.All(superheroes, hero => Assert.True(hero.IsActive));
    }

    [Fact]
    public async Task GetArchivedSuperheroes_Should_Return_Inactive_Superheroes()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/superheroes/archived");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var superheroes = JsonConvert.DeserializeObject<List<Superhero>>(content);
        
        Assert.NotNull(superheroes);
        Assert.All(superheroes, hero => Assert.False(hero.IsActive));
    }

    [Fact]
    public async Task DeleteSuperhero_Should_Mark_As_Inactive_With_DeletedAt_Timestamp()
    {
        // Arrange
        var superheroId = 1;
        
        // Act
        var response = await _httpClient.DeleteAsync($"/api/v1/superheroes/{superheroId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify the superhero is now inactive
        var getResponse = await _httpClient.GetAsync("/api/v1/superheroes");
        var content = await getResponse.Content.ReadAsStringAsync();
        var superheroes = JsonConvert.DeserializeObject<List<Superhero>>(content);
        
        var deletedHero = superheroes.FirstOrDefault(h => h.Id == superheroId);
        Assert.NotNull(deletedHero);
        Assert.False(deletedHero.IsActive);
        Assert.NotNull(deletedHero.DeletedAt);
    }

    [Fact]
    public async Task DeleteSuperhero_Should_Return_Active_Superheroes_Only_In_Response()
    {
        // Arrange
        var superheroId = 2;
        
        // Act - Delete a superhero
        await _httpClient.DeleteAsync($"/api/v1/superheroes/{superheroId}");
        
        // Get all superheroes after deletion
        var response = await _httpClient.GetAsync("/api/v1/superheroes");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var superheroes = JsonConvert.DeserializeObject<List<Superhero>>(content);
        
        // Verify that deleted superhero is not in the active list
        Assert.All(superheroes, hero => Assert.NotEqual(heroId, hero.Id));
    }

    [Fact]
    public async Task GetSuperheroes_Should_Have_Correct_Response_Structure()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/superheroes");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var superheroes = JsonConvert.DeserializeObject<List<Superhero>>(content);
        
        Assert.NotNull(superheroes);
        Assert.True(superheroes.Count >= 0); // Can be empty
        
        foreach (var hero in superheroes)
        {
            Assert.NotNull(hero.Id);
            Assert.NotNull(hero.Name);
            Assert.NotNull(hero.AlterEgo);
            Assert.NotNull(hero.IsActive);
            Assert.NotNull(hero.CreatedAt);
        }
    }

    [Fact]
    public async Task GetArchivedSuperheroes_Should_Have_Correct_Response_Structure()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/v1/superheroes/archived");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var superheroes = JsonConvert.DeserializeObject<List<Superhero>>(content);
        
        Assert.NotNull(superheroes);
        Assert.True(superheroes.Count >= 0); // Can be empty
        
        foreach (var hero in superheroes)
        {
            Assert.NotNull(hero.Id);
            Assert.NotNull(hero.Name);
            Assert.NotNull(hero.AlterEgo);
            Assert.NotNull(hero.IsActive);
            Assert.NotNull(hero.DeletedAt);
            Assert.NotNull(hero.CreatedAt);
        }
    }

    public class Superhero
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AlterEgo { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}