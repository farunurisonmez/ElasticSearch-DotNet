using Elasticsearch.API.DTOs;
using Elasticsearch.API.Repositories;
using System.Net;

namespace Elasticsearch.API.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository) { 
            _productRepository = productRepository;
        }

        public async Task<ResponseDto<ProductDto>> SaveAsync(ProductCreateDto request) {

            var responseProduct = await _productRepository.SaveAsync(request.CreateProduct());

            if (responseProduct == null) {
                return ResponseDto<ProductDto>.Fail(new List<string> { "Kayıt oluşturulamadı" }, System.Net.HttpStatusCode.InternalServerError);
            }

            return ResponseDto<ProductDto>.Success(responseProduct.CreateDto(), HttpStatusCode.Created);

        }
    }
}
