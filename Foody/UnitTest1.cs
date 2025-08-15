using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;



namespace Foody
{
    [TestFixture]
    public class FoodyTests
    {
        private RestClient client;
        private static string createdFoodId;

        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("VasiMii", "Parola12");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }
    
        [Test, Order(1)]

        public void CreateFoodWithRequiredFields_ShouldReturnSuccess()
        {
            var food = new
            {
                Name = "Pizza",
                Description = "Delicious",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createdFoodId = json.GetProperty("foodId").GetString() ?? string.Empty;

            Assert.That(createdFoodId, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(2)]
        public void EditFoodTitle_ShouldReturnOk()
        {
            var newInfo = new[]
            {
                new
                { path = "/name",
                  op = "replace",
                  value = "Update food name"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);

            request.AddJsonBody(newInfo);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllFoods_ShouldReturnList()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var foods = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(foods, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteEditedFood_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateFoodWithoutRequiredFields_ShouldReturErrorMessage()
        {
            var empty = new[]
            {
                new
                { path = "/name",
                  op = "replace",
                  value = "Update food name"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);

            request.AddJsonBody(empty);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test, Order(6)]
        public void EditNonExistingFood_ShouldReturnErrorMessage()
        {
            string invalidFoodId = "55";
            var nonExistingFood = new[]
            {
                new
                { path = "/name",
                  op = "replace",
                  value = "Update food name"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{invalidFoodId}", Method.Patch);

            request.AddJsonBody(nonExistingFood);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No food revues..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingFood_ShouldReturnErrorMessage()
        {
            string invalidFoodId = "55";
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));
        }

        [OneTimeTearDown]
        public void Cleanuo()
        {
            client?.Dispose();
        }
    }
}