#  Mango Restaurant - Net 6.0 Azure Micro Services Project

---

## Basics

### Using Azure Storage

- Login to your account in **portal.azure.com**
- Search for Storage Account and create a new one
- You could use Storage Explorer in browser or download for Linux, macOS or Windows
- In Blob containers create a new blob and upload files
- For access permissions right click to blob and set public access level to "Public read access for blobs only"

### Consuming API From MVC Layer

#### Creating API

* Create your API like always with Models, Dtos, DbContexts, IoCs, Mappers maybe even using layered architecture

#### Talking to the API 

* In Web Layer firstly add API urls to appsettings.json like 

```json
"ServiceUrls": {
    "ProductAPI": "https://localhost:5001"
}
```

- Be sure to run apps on different ports or they will conflict
- Create a class named SD to keep static data

```cs
namespace Mango.Web
{
    public static class SD
    {
        public static string ProductAPIBase { get; set; }
        public enum ApiType { GET, POST, PUT, DELETE }
    }
}
```

- Create Dtos in models folder (copy response and product dtos from other layer)

- Code repositories in Services folder with IServices inside for keeping interfaces

```cs
namespace Mango.Web.Services.IServices
{
    public interface IProductService
    {
        Task<T> GetAllProductsAsync<T>();
        Task<T> GetProductByIdAsync<T>(int id);
        Task<T> CreateProductAsync<T>(ProductDto productDto);
        Task<T> UpdateProductAsync<T>(ProductDto productDto);
        Task<T> DeleteProductAsync<T>(int id);
    }
}
```

- But coding for every type of api will be repetitive. We need to make it more generic
- Create **ApiRequest** model

```cs
namespace Mango.Web.Models
{
    public class ApiRequest
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string Url { get; set; }
        public object Data { get; set; }
        public string AccessToken { get; set; }
    }
}
```

- Create **IBaseService** in IServices

```cs
namespace Mango.Web.Services.IServices
{
    public interface IBaseService : IDisposable
    {
        ResponseDto responseModel { get; set; }
        Task<T> SendAsync<T>(ApiRequest apiRequest);
    }
}
```

- Implement it in **BaseService**

```cs
namespace Mango.Web.Services
{
    public class BaseService : IBaseService
    {
        public ResponseDto responseModel { get; set; }
        public IHttpClientFactory httpClient { get; set; }

        public BaseService(IHttpClientFactory httpClient)
        {
            responseModel = new();
            this.httpClient = httpClient;
        }

        public async Task<T> SendAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                var client = httpClient.CreateClient("MangoAPI");
                HttpRequestMessage message = new();
                message.Headers.Add("Accept", "application/json");
                message.RequestUri = new Uri(apiRequest.Url);
                client.DefaultRequestHeaders.Clear();
                if (apiRequest.Data != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                        Encoding.UTF8, "application/json");
                }

                HttpResponseMessage apiResponse = null;
                switch (apiRequest.ApiType)
                {
                    case SD.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case SD.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case SD.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }
                apiResponse = await client.SendAsync(message);

                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                var apiResponseDto = JsonConvert.DeserializeObject<T>(apiContent);
                
                return apiResponseDto;
            }
            catch (Exception e)
            {
                var dto = new ResponseDto
                {
                    DisplayMessage = "Error",
                    ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                    IsSuccess = false
                };
                var res = JsonConvert.SerializeObject(dto);
                var apiResponseDto = JsonConvert.DeserializeObject<T>(res);
                return apiResponseDto;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(true);
        }
    }
}
```

#### Creating Service and Controllers on Web for API Controllers

- Create Product Service interface and implementation class

```cs
namespace Mango.Web.Services.IServices
{
    public interface IProductService : IBaseService
    {
        Task<T> GetAllProductsAsync<T>();
        Task<T> GetProductByIdAsync<T>(int id);
        Task<T> CreateProductAsync<T>(ProductDto productDto);
        Task<T> UpdateProductAsync<T>(ProductDto productDto);
        Task<T> DeleteProductAsync<T>(int id);
    }
}
```

```cs
namespace Mango.Web.Services
{
    public class ProductService : BaseService, IProductService
    {
        private readonly IHttpClientFactory _clientFactory;

        public ProductService(IHttpClientFactory clientFactory) : base(clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<T> CreateProductAsync<T>(ProductDto productDto)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = productDto,
                Url = SD.ProductAPIBase + "/api/products",
                AccessToken = ""
            });
        }


        public async Task<T> DeleteProductAsync<T>(int id)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = SD.ProductAPIBase + "/api/products/" + id,
                AccessToken = ""
            });
        }

        public async Task<T> GetAllProductsAsync<T>()
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "/api/products/",
                AccessToken = ""
            });
        }

        public async Task<T> GetProductByIdAsync<T>(int id)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "/api/products/" + id,
                AccessToken = ""
            });
        }

        public async Task<T> UpdateProductAsync<T>(ProductDto productDto)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = productDto,
                Url = SD.ProductAPIBase + "/api/products",
                AccessToken = ""
            });
        }
    }
}
```

- Web controller should look like this. They are sending requests to API with using methods in ProductService that uses BaseService that talks to the API. API responds to these requests using our repositories

```cs
namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService){
            _productService = productService;
        }

        public async Task<IActionResult> ProductIndex(){
            List<ProductDto> list = new();
            var response = await _productService.GetAllProductsAsync<ResponseDto>();
            if (response != null && response.IsSuccess) list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));

            return View(list);
        }

        public async Task<IActionResult> ProductCreate()
        {
            return await Task.Run(() => View());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductCreate(ProductDto model){
            if (ModelState.IsValid){
                var response = await _productService.CreateProductAsync<ResponseDto>(model);
                if (response != null && response.IsSuccess) return RedirectToAction(nameof(ProductIndex));
            }
            return View(model);
        }

        public async Task<IActionResult> ProductEdit(int productId){
            if (ModelState.IsValid){
                var response = await _productService.GetProductByIdAsync<ResponseDto>(productId);
                if (response != null && response.IsSuccess){
                    ProductDto model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                    return View(model);
                }
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductEdit(ProductDto model)
        {
            if (ModelState.IsValid){
                var response = await _productService.UpdateProductAsync<ResponseDto>(model);
                if (response != null && response.IsSuccess) return RedirectToAction(nameof(ProductIndex));
            }
            return View(model);
        }     

        public async Task<IActionResult> ProductDelete(int productId){
            if (ModelState.IsValid){
                var response = await _productService.GetProductByIdAsync<ResponseDto>(productId);
                if (response != null && response.IsSuccess){
                    ProductDto model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                    return View(model);
                }
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductDelete(ProductDto model){
            if (ModelState.IsValid){
                var response = await _productService.DeleteProductAsync<ResponseDto>(model.ProductId);
                if (response.IsSuccess) return RedirectToAction(nameof(ProductIndex));
            }
            return View(model);
        }
    }
}
```

### Identity Server

#### Integrate Quickstart Identity Server

- Install Identity Server dotnet template

```bash
dotnet new --install Duende.IdentityServer.Templates
```

- In terminal cd to the main project dir and create the quickstart template for identity server

```bash
dotnet new isui
```

- Now we got the template files for our identity server in our projects root folder. Let's create a new MVC Project inside services folder and move the files to corresponding places
- Create SD for keeping static files
- 
- README WORK IN PROGRESS
