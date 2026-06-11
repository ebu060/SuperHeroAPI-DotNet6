using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Tests.ConsumerTests
{
    public class ApiContractConsumerTests
    {
        private readonly HttpClient _httpClient;
        
        public ApiContractConsumerTests()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri("https://localhost:5001/");
        }

        [Fact]
        public async Task UpdateHero_Endpoint_Should_Accept_SuperPower_Field()
        {
            // Arrange
            var heroId = 1;
            var updateHeroRequest = new
            {
                Id = heroId,
                Name = "Superman",
                FirstName = "Clark",
                LastName = "Kent",
                HeroName = "Superman",
                SuperPower = "Flight" // New field added to contract
            };

            var jsonContent = JsonSerializer.Serialize(updateHeroRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync($"api/v1/superheroes/{heroId}", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateHero_Endpoint_Should_Comply_With_Updated_Schema()
        {
            // Arrange
            var heroId = 2;
            var updateHeroRequest = new
            {
                Id = heroId,
                Name = "Batman",
                FirstName = "Bruce",
                LastName = "Wayne",
                HeroName = "Batman",
                SuperPower = null // Optional field can be null
            };

            var jsonContent = JsonSerializer.Serialize(updateHeroRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync($"api/v1/superheroes/{heroId}", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateHero_Endpoint_Should_Validate_SuperPower_Field_Exists()
        {
            // Arrange
            var heroId = 3;
            var updateHeroRequest = new
            {
                Id = heroId,
                Name = "Spider-Man",
                FirstName = "Peter",
                LastName = "Parker",
                HeroName = "Spider-Man",
                SuperPower = "Web Shooting" // New field present in request
            };

            var jsonContent = JsonSerializer.Serialize(updateHeroRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync($"api/v1/superheroes/{heroId}", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateHero_Endpoint_Should_Handle_Missing_SuperPower_Field()
        {
            // Arrange
            var heroId = 4;
            var updateHeroRequest = new
            {
                Id = heroId,
                Name = "Wonder Woman",
                FirstName = "Diana",
                LastName = "Prince",
                HeroName = "Wonder Woman"
                // SuperPower field intentionally omitted (backward compatible)
            };

            var jsonContent = JsonSerializer.Serialize(updateHeroRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync($"api/v1/superheroes/{heroId}", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}