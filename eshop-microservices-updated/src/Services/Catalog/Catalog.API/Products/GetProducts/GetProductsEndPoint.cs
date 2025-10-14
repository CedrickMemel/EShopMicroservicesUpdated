namespace Catalog.API.Products.GetProducts;

//public record GetProductsREquest();

public record GetProductsResponse(IEnumerable<Product> Products);

public class GetProductsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ISender sender) =>
        {
            var result = await sender.Send(new GetProductsQuery());
            var response = result.Adapt<GetProductsResponse>();
            return Results.Ok(response);
        }).WithName("GetProducts")
        .WithDescription("Get Products")
        .Produces<GetProductsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Products");
    }
}
