using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TennisBookings.Merchandise.Api.Data;
using TennisBookings.Merchandise.Api.Models.Output;
using TennisBookings.Merchandise.Api.Stock;

namespace TennisBookings.Merchandise.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IProductDataRepository _productDataRepository;
        private readonly IStockCalculator _stockCalculator;

        public StockController(IProductDataRepository productDataRepository, IStockCalculator stockCalculator)
        {
            _productDataRepository = productDataRepository;
            _stockCalculator = stockCalculator;
        }

        [HttpGet("total")]
        public async Task<ActionResult<StockTotalOutputModel>> Total()
        {
            var products = await _productDataRepository.GetProductsAsync();
            var totalStockCount = _stockCalculator.CalculateStockTotal(products);

            return Ok(new StockTotalOutputModel { StockItemTotal = totalStockCount });
        }
    }
}
