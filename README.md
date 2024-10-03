# Elastic Search with DotNet

Bu proje, **ElasticSearch** kullanarak ürün bilgilerini (Product) saklayan ve dışarıya bir **API** sunan bir sistemdir. Kullanıcılar, bu API aracılığıyla ürün ekleyebilir, silebilir ya da güncelleyebilir. Proje, tek katmanlı bir mimariye sahiptir ve ElasticSearch ile Kibana'yı kolayca çalıştırmak için **Docker Compose** kullanır.

## Proje Mimarisi

Proje tek katmanlı bir yapıya sahiptir; yani iş mantığı, veri erişimi ve API katmanları tek bir katmanda yer almaktadır. 

- **ElasticSearch**: Bir arama motoru olarak kullanılan ElasticSearch, projede veritabanı gibi çalışmaktadır. SQL veritabanları gibi tablolardan değil, belgelerden (documents) oluşan bir yapıdadır.
- **Kibana**: ElasticSearch ile görsel analizler yapabilmek için kullanılan bir arayüzdür.

---

## Docker ile ElasticSearch ve Kibana Çalıştırma

Bu projede ElasticSearch ve Kibana'yı hızlı bir şekilde çalıştırmak için **Docker Compose** kullanıyoruz. Aşağıdaki `docker-compose.yml` dosyası ElasticSearch ve Kibana servislerini ayağa kaldırır:

```yaml
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.7.1
    expose:
      - 9200
    environment:
      - xpack.security.enabled=false
      - "discovery.type=single-node"
      - ELASTIC_USERNAME=elastic
      - ELASTIC_PASSWORD=DkIedPPSCb
    networks:
      - es-net
    ports:
      - 9200:9200
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data
  kibana:
    image: docker.elastic.co/kibana/kibana:8.7.1
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    expose:
      - 5601
    networks:
      - es-net
    depends_on:
      - elasticsearch
    ports:
      - 5601:5601
    volumes:
      - kibana-data:/usr/share/kibana/data
networks:
  es-net:
    driver: bridge
volumes:
  elasticsearch-data:
    driver: local
  kibana-data:
    driver: local
```

### ElasticSearch ve Kibana'yı Başlatma

Aşağıdaki komut ile ElasticSearch ve Kibana servislerini başlatabilirsiniz:

```bash
docker-compose up -d
```

Bu komutu çalıştırdıktan sonra:

- **ElasticSearch** `http://localhost:9200/` adresinde çalışıyor olacak.
- **Kibana** ise `http://localhost:5601/` adresinden erişilebilir hale gelecektir.

### Kibana ile E-Commerce Sample Data Entegrasyonu

Kibana'nın arayüzüne giriş yaptıktan sonra, **[eCommerce] Revenue** örnek veri setini entegre edebilir ve bu veri seti üzerinden aramalar yaparak ElasticSearch'ün gücünü test edebilirsiniz.

---

## Proje Detayları

Aşağıda sağladığınız kodlarla ilgili detaylı bir analiz ve açıklama yer almaktadır. Proje, Elasticsearch kullanarak ürün yönetim sistemini uygulayan bir API'yi içermektedir. Her bir dosya için ana işlevler ve yapılar hakkında bilgi verilecektir.

### 1. **Services/ProductService.cs**
`ProductService` sınıfı, ürünlerle ilgili iş mantığını içerir. CRUD (Create, Read, Update, Delete) işlemleri için gerekli metotları sağlar.

- **Fieldlar:**
  - `_productRepository`: Ürün veritabanı işlemlerini gerçekleştiren `ProductRepository` nesnesi.
  - `_logger`: Loglama işlemleri için kullanılan logger.

- **Constructor:** `ProductRepository` ve `ILogger` nesnelerini alarak başlatılır.

- **Metotlar:**
  - `SaveAsync(ProductCreateDto request)`: Yeni bir ürün kaydetmek için kullanılır. Eğer kayıt başarılı olursa, başarı durumu ile birlikte ürün bilgilerini döner, aksi takdirde hata mesajı ile döner.
  
  - `GetAllAsync()`: Tüm ürünleri getirir ve ürünlerin özelliklerini `ProductDto` formatında döner.

  - `GetByIdAsync(string id)`: Belirtilen ID'ye sahip ürünü getirir. Eğer ürün yoksa, 404 hatası döner.

  - `UpdateAsync(ProductUpdateDto updateProduct)`: Mevcut bir ürünü günceller. Güncelleme işlemi başarılı olursa 204 No Content döner, aksi takdirde hata mesajı ile döner.

  - `DeleteAsync(string id)`: Belirtilen ID'ye sahip ürünü siler. Silme işlemi başarısız olursa hata mesajı ile döner.

### 2. **Repositories/ProductRepository.cs**
`ProductRepository` sınıfı, Elasticsearch ile etkileşim kurarak ürün veritabanı işlemlerini gerçekleştiren bir katmandır.

- **Fieldlar:**
  - `_client`: Elasticsearch istemcisi.
  - `indexName`: Ürünlerin saklandığı indeksin adı.

- **Metotlar:**
  - `SaveAsync(Product newProduct)`: Yeni bir ürünü Elasticsearch'e kaydeder. Başarılıysa ürün nesnesini döner.

  - `GetAllAsync()`: Tüm ürünleri getirir ve `ImmutableList<Product>` formatında döner.

  - `GetByIdAsync(string id)`: Belirtilen ID'ye sahip ürünü getirir.

  - `UpdateAsync(ProductUpdateDto updateProduct)`: Mevcut bir ürünü günceller.

  - `DeleteAsync(string id)`: Belirtilen ID'ye sahip ürünü siler ve sonucu döner.

### 3. **Models/EColor.cs**
`EColor` enum'ı, ürün özelliklerinde kullanılacak renk seçeneklerini tanımlar. Kırmızı, yeşil ve mavi renkleri içerir.

### 4. **Models/Product.cs**
`Product` sınıfı, ürün verilerini tutar.

- **Fieldlar:**
  - `Id`, `Name`, `Price`, `Stock`, `Created`, `Updated`: Ürün bilgileri.
  - `Feature`: Ürün özelliklerini temsil eden `ProductFeature` nesnesi.

- **Metotlar:**
  - `CreateDto()`: Ürün bilgilerini `ProductDto` formatında döner.

### 5. **Models/ProductFeature.cs**
`ProductFeature` sınıfı, ürünün fiziksel özelliklerini (genişlik, yükseklik, renk) tutar.

### 6. **Extensions/Elasticsearch.cs**
`ElasticsearchExt` sınıfı, Elasticsearch istemcisini uygulama servisine eklemek için bir genişletme metodu içerir.

- **Metot:**
  - `AddElastic()`: Elasticsearch ayarlarını alır ve uygulama hizmetlerine ekler.

### 7. **DTOs/ProductCreateDto.cs**
`ProductCreateDto` sınıfı, yeni bir ürün oluşturmak için gerekli verileri taşır.

- **Metot:**
  - `CreateProduct()`: DTO'dan bir `Product` nesnesi oluşturur.

### 8. **DTOs/ProductDto.cs**
`ProductDto` sınıfı, ürün verilerini istemciye göndermek için kullanılır.

### 9. **DTOs/ProductFeatureDto.cs**
`ProductFeatureDto` sınıfı, ürün özelliklerini istemciye göndermek için kullanılır.

### 10. **DTOs/ProductUpdateDto.cs**
`ProductUpdateDto` sınıfı, mevcut bir ürünü güncellemek için gerekli verileri taşır.

### 11. **DTOs/ResponseDto.cs**
`ResponseDto<T>` sınıfı, API yanıtlarını standart hale getirmek için kullanılır. Başarılı veya hata durumlarında döndürülecek verileri kapsar.

- **Metotlar:**
  - `Success()`: Başarılı bir yanıt döner.
  - `Fail()`: Hata durumunda yanıt döner.

### 12. **Controllers/BaseController.cs**
`BaseController` sınıfı, diğer controller'lar için temel işlevsellik sağlar.

- **Metot:**
  - `CreateActionResult<T>()`: API yanıtını standart hale getirir.

### 13. **Controller/ProductsController.cs**
`ProductsController` sınıfı, ürünlerle ilgili API uç noktalarını yönetir.

- **Metotlar:**
  - `Save(ProductCreateDto request)`: Yeni bir ürün kaydetmek için kullanılır.
  - `GetAll()`: Tüm ürünleri getirmek için kullanılır.
  - `GetById(string id)`: Belirtilen ID'ye sahip ürünü getirmek için kullanılır.
  - `Update(ProductUpdateDto request)`: Mevcut bir ürünü güncellemek için kullanılır.
  - `Delete(string id)`: Belirtilen ID'ye sahip ürünü silmek için kullanılır.

---

### **Genel Kullanım Amaçları:**
- **CRUD İşlemleri:** Uygulama, ürünleri eklemek, güncellemek, silmek ve listelemek için tam bir API sunmaktadır.
- **Elasticsearch Entegrasyonu:** Veriler, hızlı arama ve veri işleme için Elasticsearch ile etkileşimli bir şekilde yönetilmektedir.

