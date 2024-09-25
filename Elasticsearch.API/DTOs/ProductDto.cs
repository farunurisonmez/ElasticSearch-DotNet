using Elasticsearch.API.Models;
using Nest;

namespace Elasticsearch.API.DTOs
{
    public record ProductDto(
        string Id,
        string Name,
        decimal Price,
        int Stock,
        ProductFeatureDto? Feature
        )
    {
        private string ıd;
        private ProductFeatureDto productFeatureDto;

        public ProductDto(string ıd, decimal price, int stock, ProductFeatureDto productFeatureDto)
        {
            this.ıd = ıd;
            Price = price;
            Stock = stock;
            this.productFeatureDto = productFeatureDto;
        }
    }
}
