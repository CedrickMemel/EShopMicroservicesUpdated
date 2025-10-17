using Catalog.API.Exceptions;

namespace Catalog.API.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;

public record GetProductByIdResult(Product Product);
internal class GetProductByIdQueryHandler(IDocumentSession sesion) : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await sesion.LoadAsync<Product>(query.Id, cancellationToken);

        if (result is null)
        {
            throw new ProductNotFoundException(query.Id);
        }

        // ArgumentNullException.ThrowIfNull(result);

        return new GetProductByIdResult(result);
    }
}
