using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json.Linq;

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
        public async Task GetSuperheroesPaged_Endpoint_Exists_And_Returns_Valid_Response()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Query_Parameters_Returns_Valid_Response()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=10&universe=Marvel&minPowerLevel=50&sortBy=Name&sortOrder=asc");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Minimal_Query_Parameters_Returns_Valid_Response()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=5");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_Returns_Expected_Properties_In_Response()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=5");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var jsonObject = JObject.Parse(jsonResponse);
            
            Assert.Contains("items", jsonObject.Properties().Select(p => p.Name));
            Assert.Contains("totalItems", jsonObject.Properties().Select(p => p.Name));
            Assert.Contains("currentPage", jsonObject.Properties().Select(p => p.Name));
            Assert.Contains("totalPages", jsonObject.Properties().Select(p => p.Name));
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Invalid_PageSize_Returns_BadRequest()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=0");
            
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Negative_Page_Returns_BadRequest()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=-1&pageSize=5");
            
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Unsupported_SortOrder_Returns_BadRequest()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=5&sortOrder=invalid");
            
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Unsupported_Universe_Returns_BadRequest()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=5&universe=InvalidUniverse");
            
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Minimum_Power_Level_Returns_Valid_Response()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=5&minPowerLevel=10");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Sort_By_Power_Level_Returns_Valid_Response()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=5&sortBy=PowerLevel&sortOrder=desc");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetSuperheroesPaged_Endpoint_With_Sort_By_Name_Returns_Valid_Response()
        {
            // Act
            var response = await _httpClient.GetAsync("api/v1/superheroes/paged?page=1&pageSize=5&sortBy=Name&sortOrder=asc");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }
}