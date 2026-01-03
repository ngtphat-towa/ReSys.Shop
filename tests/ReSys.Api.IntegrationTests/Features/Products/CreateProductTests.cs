using FluentAssertions;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Features.Products.CreateProduct;
using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using System.Text;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class CreateProductTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "POST /api/products: Should create a new product")]
    public async Task Post_WithValidRequest_CreatesProduct()
    {
        var request = new CreateProduct.Request { Name = "NewProduct", Description = "Desc", Price = 10 };

        var response = await Client.PostAsync("/api/products", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonConvert.DeserializeObject<ProductDetail>(content, JsonSettings);
        
        product!.Name.Should().Be("NewProduct");
        product.Id.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "POST /api/products: Should return Created with correct Location header")]
    public async Task Post_ReturnsCorrectLocation()
    {
        var request = new CreateProduct.Request { Name = "LocationTest", Description = "D", Price = 1 };

        var response = await Client.PostAsync("/api/products", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();
        var product = JsonConvert.DeserializeObject<ProductDetail>(content, JsonSettings);
        
        response.Headers.Location!.ToString().Should().Be($"/api/products/{product!.Id}");
    }

    [Fact(DisplayName = "POST /api/products: Should return Conflict for duplicate name")]
    public async Task Post_WithDuplicateName_ReturnsConflict()
    {
        var name = "DuplicateTest";
        var r1 = new CreateProduct.Request { Name = name, Description = "D", Price = 1 };
        await Client.PostAsync("/api/products", new StringContent(JsonConvert.SerializeObject(r1, JsonSettings), Encoding.UTF8, "application/json"));

        var r2 = new CreateProduct.Request { Name = name, Description = "D", Price = 2 };
        var response = await Client.PostAsync("/api/products", new StringContent(JsonConvert.SerializeObject(r2, JsonSettings), Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "POST /api/products: Should return BadRequest for invalid input (e.g. empty name)")]
    public async Task Post_WithInvalidRequest_ReturnsBadRequest()
    {
        var request = new CreateProduct.Request { Name = "", Description = "D", Price = -1 };

        var response = await Client.PostAsync("/api/products", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
