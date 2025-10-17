namespace Catalog.API.Products.GetProducByCategory;

public record GetProductByCategoryQuery(string category) : IQuery<GetProducByCategoryResult>;

public record GetProducByCategoryResult(IEnumerable<Product> Products);
internal class GetProductByCategoryQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductByCategoryQuery, GetProducByCategoryResult>
{
    public async Task<GetProducByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
    {
        var products = await session.Query<Product>()
            .Where(p => p.Category.Contains(query.category))
            .ToListAsync(token: cancellationToken);

        return new GetProducByCategoryResult(products);
    }
}
