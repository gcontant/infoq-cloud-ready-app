using Microsoft.EntityFrameworkCore;
using ProductCatalog.Data;
using ProductCatalog.Exceptions;
using ProductCatalog.Models;
using ProductCatalog.Models.Entities;

namespace ProductCatalog.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductCatalogDbContext _dbContext;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        ProductCatalogDbContext dbContext,
        ILogger<ProductService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product(
            request.Name,
            request.Price,
            request.Owner);

        _dbContext.Products.Add(product);

        await _dbContext.SaveChangesAsync();

        return product.Id; // Generated at the SaveChangesAsync
    }

    public async Task DeleteProductAsync(Guid id)
    {
        Product product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            throw new EntityNotFoundException();

        _dbContext.Products.Remove(product);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
    {
        List<Product> products = await _dbContext.Products.ToListAsync();

        var response = new List<ProductResponse>();

        foreach (Product product in products)
        {
            var productResponse = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
            };

            response.Add(productResponse);
        }

        return response;
    }

    public async Task<ProductDetailsResponse> GetProductAsync(Guid id)
    {
        Product product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            throw new EntityNotFoundException();

        var response = new ProductDetailsResponse
        {
            Id = product.Id,
            Name = product.Name,
            Owner = product.Owner,
            Price = product.Price,
        };

        return response;
    }

    public async Task UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        Product product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            throw new EntityNotFoundException();

        product.UpdateOwner(request.Owner);
        product.UpdatePrice(request.Price);
        product.UpdateName(request.Name);

        _dbContext.Products.Update(product);

        await _dbContext.SaveChangesAsync();
    }
    }
}