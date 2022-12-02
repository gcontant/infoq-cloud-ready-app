
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

        /// <summary>
        /// Get the given product
        /// </summary>
        /// <remarks>
        /// <para>Sample request:</para>
        /// <para>    GET /api/product/{id}</para>
        /// </remarks>
        /// <param name="id">Product id</param>
        /// <response code="200">Product details</response>
        [HttpGet]
        [Route("product")]
        [ProducesResponseType(typeof(IEnumerable<ProductDetailsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProducts()
        {
            var dtos = await _productService.GetAllProductsAsync();
            return Ok(dtos);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(
            [FromRoute] Guid id)
        {
            var dto = await _productService.GetProductAsync(id);
            return Ok(dto);
        }

        [HttpPost]
        [Route("product")]
        public async Task<IActionResult> AddProduct(
            [FromBody] CreateProductRequest request)
        {
            Guid productId = await _productService.CreateProductAsync(request);

            Response.Headers.Add("Location", productId.ToString());
            return NoContent();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateProduct(
            [FromRoute] Guid id,
            [FromBody] UpdateProductRequest request)
        {
            await _productService.UpdateProductAsync(id, request);
            return NoContent();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteProduct(
            [FromRoute] Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok();
        }
    }
}