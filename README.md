 Bu proje, **ElasticSearch** kullanarak ürün bilgilerini (Product) saklıyor ve dışarıya bir **API** sunuyor. Kullanıcılar bu API aracılığıyla ürün ekleyebiliyor, silebiliyor ya da güncelleyebiliyor.

**ElasticSearch** burada bir veritabanı gibi çalışıyor, ama SQL veritabanları gibi tablolardan oluşan bir yapıda değil. Daha çok belgeler (documents) tutan bir arama motoru gibi düşünülebilir.

---

### Temel Kavramlar

1. **Model (Product, ProductFeature)**: Gerçek dünyadaki ürünlerin bilgilerini (isim, fiyat, stok gibi) temsil eden sınıflar.
2. **Repository**: Ürünlerin veritabanına (ElasticSearch) nasıl kaydedileceğini ya da geri alınacağını yöneten sınıf.
3. **Service**: İş mantığını yönetir, yani ürünlerin nasıl kaydedileceğini, hangi koşullarda hatalar vereceğini gibi kuralları içerir.
4. **Controller**: API'nin dış dünyayla konuştuğu yer. Dışarıdan gelen istekleri alır ve servisle iş birliği yaparak cevap verir.

---

### Adım Adım Anlayalım

#### 1. **Product Modeli**

İlk olarak, bir ürünün neye benzediğini düşünelim. Bir ürün (Product), ismi olan, fiyatı ve stoğu olan bir nesne. Ayrıca bir de genişlik, yükseklik ve renk gibi ek özellikleri olabilir. Biz bu özellikleri **Product** ve **ProductFeature** modelleriyle tanımladık.

```csharp
public class Product
{
    public string Id { get; set; }  // Her ürünün benzersiz bir kimliği olacak
    public string Name { get; set; } // Ürün adı
    public decimal Price { get; set; } // Ürün fiyatı
    public int Stock { get; set; }  // Ürünün stok durumu
    public ProductFeature? Feature { get; set; } // Ek ürün özellikleri (Genişlik, yükseklik gibi)
}
```

- **Product** sınıfı temel ürün bilgilerini tutuyor.
- **ProductFeature** ise ürünün ek özelliklerini içeriyor (boyutlar, renk).

#### 2. **DTO (Data Transfer Object)**

DTO'lar, API'ye gelen ya da API'den çıkan veriyi daha basit hale getirmek için kullanılır. Örneğin, dışarıya bir ürün dönerken bu ürünü bir DTO ile döndürüyoruz.

```csharp
public record ProductDto(
    string Id,
    string Name,
    decimal Price,
    int Stock,
    ProductFeatureDto? Feature
);
```

- **ProductDto** sınıfı, dışarıya döneceğimiz ürün bilgisini temsil ediyor.
- **ProductCreateDto** ise bir ürün oluşturulurken (POST isteğiyle) alınacak veriyi temsil ediyor.

#### 3. **Repository (Veritabanı ile İletişim)**

Şimdi, veritabanına (ElasticSearch'e) ürünleri nasıl kaydedeceğiz? İşte burada **ProductRepository** devreye giriyor. Bu sınıf, ElasticSearch ile iletişim kurarak ürün kaydetme, güncelleme ve silme işlemlerini yönetiyor.

```csharp
public class ProductRepository
{
    private readonly ElasticClient _client; // ElasticSearch ile iletişim kurmak için kullanılan nesne

    public ProductRepository(ElasticClient client) {
        _client = client;
    }

    public async Task<Product?> SaveAsync(Product newProduct){
        var response = await _client.IndexAsync(newProduct, x => x.Index("products"));
        if(!response.IsValid) return null;
        return newProduct;
    }
}
```

Bu sınıfın görevi:
- **ElasticClient** ile ElasticSearch'e bağlanmak.
- Ürünü veritabanına kaydetmek (Index işlemi).

#### 4. **Service (İş Mantığı)**

**Service** sınıfı işin kalbidir. Burada ürünlerin nasıl kaydedileceğini ve işlemlerin başarılı ya da başarısız olduğunda ne yapacağını tanımlıyoruz. Yani iş mantığımızı buraya yazıyoruz.

```csharp
public class ProductService
{
    private readonly ProductRepository _productRepository;

    public ProductService(ProductRepository productRepository) { 
        _productRepository = productRepository;
    }

    public async Task<ResponseDto<ProductDto>> SaveAsync(ProductCreateDto request) {
        var responseProduct = await _productRepository.SaveAsync(request.CreateProduct());
        if (responseProduct == null) {
            return ResponseDto<ProductDto>.Fail(new List<string> { "Kayıt oluşturulamadı" }, HttpStatusCode.InternalServerError);
        }
        return ResponseDto<ProductDto>.Success(responseProduct.CreateDto(), HttpStatusCode.Created);
    }
}
```

Bu sınıfın görevi:
- Gelen ürünü repository aracılığıyla veritabanına kaydetmek.
- Eğer işlem başarısız olursa bir hata mesajı döndürmek.
- İşlem başarılıysa DTO formatında ürünü döndürmek.

#### 5. **Controller (API'nin Dış Dünya ile Konuştuğu Yer)**

Son adımda API'den gelecek istekleri alacak bir **Controller** oluşturuyoruz. Dış dünyadaki birisi API'ye bir ürün kaydetmek istediğinde, bu isteği karşılayıp **ProductService** aracılığıyla ürünü kaydediyoruz.

```csharp
[HttpPost]
public async Task<IActionResult> Save(ProductCreateDto request)
{
    return Ok(await _productService.SaveAsync(request));
}
```

- Dış dünyadan (örneğin, bir mobil uygulamadan) gelen POST isteğini karşılıyor.
- Gelen ürün bilgisini servis aracılığıyla kaydediyor.
- Sonuç olarak, başarılı bir şekilde kaydedilen ürünü geri döndürüyor.