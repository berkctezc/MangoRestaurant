using System.Net.Http;
using System.Threading.Tasks;
using Mango.Web.Models.Dto;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CartService : BaseService, ICartService
    {
        public CartService(IHttpClientFactory httpClient) : base(httpClient)
        {
        }

        public async Task<T> GetCartByUserIdAsync<T>(string userId, string token = null)
        {
            throw new System.NotImplementedException();
        }

        public async Task<T> AddToCartAsync<T>(CartDto cartDto, string token = null)
        {
            throw new System.NotImplementedException();
        }

        public async Task<T> UpdateCartAsync<T>(CartDto cartDto, string token = null)
        {
            throw new System.NotImplementedException();
        }

        public async Task<T> RemoveFromCartAsync<T>(int cartId, string token = null)
        {
            throw new System.NotImplementedException();
        }
    }
}