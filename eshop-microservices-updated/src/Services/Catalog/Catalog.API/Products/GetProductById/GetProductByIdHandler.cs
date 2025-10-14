using Catalog.API.Exceptions;

namespace Catalog.API.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;

public record GetProductByIdResult(Product Product);
internal class GetProductByIdQueryHandler(IDocumentSession sesion, ILogger<GetProductByIdQueryHandler> logger) : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetProductByIdQueryHandler.Handle called with {@Query}", query);
        var result = await sesion.LoadAsync<Product>(query.Id, cancellationToken);

        if (result is null)
        {
            throw new ProductNotFoundException();
        }

        // ArgumentNullException.ThrowIfNull(result);

        return new GetProductByIdResult(result);
    }
}
