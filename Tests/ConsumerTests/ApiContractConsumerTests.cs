using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Tests.ConsumerTests
{
    public class ApiContractConsumerTests
    {
        private readonly HttpClient _httpClient;
        
        public ApiContractConsumerTests()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:5001/");
        }

        [Fact]
        public async Task UpdateHero_Endpoint_Contains_SuperPower_Field()
        {
            // Arrange
            var heroId = 1;
            var updateHeroRequest = new
            {
                Name = "Superman",
                Age = 35,
                SuperPower = "Flight"
            };

            var jsonContent = JsonConvert.SerializeObject(updateHeroRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync($"api/v1/superheroes/{heroId}", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            
            // Verify the response contains the SuperPower field
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            Assert.NotNull(responseObject.SuperPower);
            Assert.Equal("Flight", responseObject.SuperPower.ToString());
        }

        [Fact]
        public async Task UpdateHero_Endpoint_RequestBody_Contains_SuperPower_Field()
        {
            // Arrange
            var heroId = 2;
            var updateHeroRequest = new
            {
                Name = "Batman",
                Age = 40,
                SuperPower = "Intelligence"
            };

            var jsonContent = JsonConvert.SerializeObject(updateHeroRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync($"api/v1/superheroes/{heroId}", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            
            // Verify the request body schema includes SuperPower field
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            Assert.NotNull(responseObject.SuperPower);
            Assert.Equal("Intelligence", responseObject.SuperPower.ToString());
        }

        [Fact]
        public async Task UpdateHero_Endpoint_Supports_SuperPower_Field_With_Null_Value()
        {
            // Arrange
            var heroId = 3;
            var updateHeroRequest = new
            {
                Name = "Wonder Woman",
                Age = 30,
                SuperPower = (string)null
            };

            var jsonContent = JsonConvert.SerializeObject(updateHeroRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync($"api/v1/superheroes/{heroId}", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            
            // Verify the SuperPower field can be null
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            Assert.Null(responseObject.SuperPower);
        }

        [Fact]
        public async Task UpdateHero_Endpoint_Supports_SuperPower_Field_With_Empty_String()
        {
            // Arrange
            var heroId = 4;
            var updateHeroRequest = new
            {
                Name = "Flash",
                Age = 25,
                SuperPower = ""
            };

            var jsonContent = JsonConvert.SerializeObject(updateHeroRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync($"api/v1/superheroes/{heroId}", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            
            // Verify the SuperPower field can be empty string
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            Assert.Equal("", responseObject.SuperPower.ToString());
        }
    }
}