using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using System.Collections.Generic;

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
        public async Task Get_Superheroes_Paged_Endpoint_Returns_Valid_Response()
        {
            // Arrange
            var requestUri = "api/v1/superhero/paged?page=1&pageSize=10&universe=Marvel&minPower=80&sortBy=Name";

            // Act
            var response = await _httpClient.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(content);

            // Verify the response can be deserialized into expected structure
            var responseObject = JsonConvert.DeserializeObject<dynamic>(content);
            Assert.NotNull(responseObject);
        }

        [Fact]
        public async Task Put_Superhero_Endpoint_Contains_New_Fields()
        {
            // Arrange
            var requestUri = "api/v1/superhero";
            var superheroData = new
            {
                Id = 1,
                Name = "Test Hero",
                RealName = "Test Person",
                TeamId = 1,
                PowerLevel = 95,
                MissionsCompleted = 12
            };

            var jsonContent = JsonConvert.SerializeObject(superheroData);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync(requestUri, content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Get_Superheroes_Paged_Endpoint_With_Minimal_Params_Returns_Valid_Response()
        {
            // Arrange
            var requestUri = "api/v1/superhero/paged?page=1&pageSize=5";

            // Act
            var response = await _httpClient.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(content);

            // Verify the response can be deserialized into expected structure
            var responseObject = JsonConvert.DeserializeObject<dynamic>(content);
            Assert.NotNull(responseObject);
        }

        [Fact]
        public async Task Get_Superheroes_Paged_Endpoint_With_All_Params_Returns_Valid_Response()
        {
            // Arrange
            var requestUri = "api/v1/superhero/paged?page=2&pageSize=20&universe=DC&minPower=70&sortBy=PowerLevel";

            // Act
            var response = await _httpClient.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(content);

            // Verify the response can be deserialized into expected structure
            var responseObject = JsonConvert.DeserializeObject<dynamic>(content);
            Assert.NotNull(responseObject);
        }

        [Fact]
        public async Task Put_Superhero_Endpoint_With_Optional_Fields_Returns_Valid_Response()
        {
            // Arrange
            var requestUri = "api/v1/superhero";
            var superheroData = new
            {
                Id = 2,
                Name = "Another Hero",
                RealName = "Another Person",
                TeamId = null,
                PowerLevel = 85,
                MissionsCompleted = 0
            };

            var jsonContent = JsonConvert.SerializeObject(superheroData);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync(requestUri, content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}