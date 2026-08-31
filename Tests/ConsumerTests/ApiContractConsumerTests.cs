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
            _httpClient.BaseAddress = new Uri("http://localhost:5000/");
        }

        [Fact]
        public async Task PostMissions_ReturnsCreatedResponse()
        {
            // Arrange
            var missionRequest = new
            {
                title = "Save the World",
                description = "A critical mission to save Earth",
                difficulty = 5
            };

            var json = JsonConvert.SerializeObject(missionRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync("/api/v1/missions", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task PostMissionsAssignHeroes_ReturnsOkResponse()
        {
            // Arrange
            var assignRequest = new
            {
                heroIds = new[] { 1, 2, 3 }
            };

            var json = JsonConvert.SerializeObject(assignRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync("/api/v1/missions/1/assign-heroes", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task PostMissionsComplete_ReturnsOkResponse()
        {
            // Arrange
            var completeRequest = new
            {
                completionDate = DateTime.UtcNow,
                success = true
            };

            var json = JsonConvert.SerializeObject(completeRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync("/api/v1/missions/1/complete", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task PostTeams_ReturnsCreatedResponse()
        {
            // Arrange
            var teamRequest = new
            {
                name = "Avengers",
                universe = "Marvel"
            };

            var json = JsonConvert.SerializeObject(teamRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync("/api/v1/teams", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task PostTeamsAddHero_ReturnsOkResponse()
        {
            // Act
            var response = await _httpClient.PostAsync("/api/v1/teams/1/add-hero/1", null);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetHeroesPaged_ReturnsOkResponseWithPagination()
        {
            // Act
            var response = await _httpClient.GetAsync("/api/v1/heroes/paged?page=1&pageSize=10");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetHeroesPaged_WithFilters_ReturnsOkResponse()
        {
            // Act
            var response = await _httpClient.GetAsync("/api/v1/heroes/paged?page=1&pageSize=5&filter=name:superman");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetHeroesPaged_WithSorting_ReturnsOkResponse()
        {
            // Act
            var response = await _httpClient.GetAsync("/api/v1/heroes/paged?page=1&pageSize=5&sort=name:asc");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}