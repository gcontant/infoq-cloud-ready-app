
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Models;
using ProductCatalog.Services;

namespace ProductCatalog.Controllers
{
    /// <summary>
    /// APIs to manage products
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("api/product/")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(
            IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDetailsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProducts()
        {
            var dtos = await _productService.GetAllProductsAsync();
            return Ok(dtos);
        }

        /// <summary>
        /// Get the given product
        /// </summary>
        /// <remarks>
        /// <para>Sample request:</para>
        /// <para>    GET /api/product/{id}</para>
        /// </remarks>
        /// <param name="id">Product id</param>
        /// <response code="200">Product details</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(
            [FromRoute] Guid id)
        {
            var dto = await _productService.GetProductAsync(id);
            return Ok(dto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddProduct(
            [FromBody] CreateProductRequest request)
        {
            Guid productId = await _productService.CreateProductAsync(request);

            Response.Headers.Add("Location", productId.ToString());
            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(
            [FromRoute] Guid id,
            [FromBody] UpdateProductRequest request)
        {
            await _productService.UpdateProductAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(
            [FromRoute] Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok();
        }
    }
}