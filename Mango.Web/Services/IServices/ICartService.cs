﻿using System.Threading.Tasks;
using Mango.Web.Models.Dto;

namespace Mango.Web.Services.IServices
{
    public interface ICartService : IBaseService
    {
        Task<T> GetCartByUserIdAsync<T>(string userId, string token = null);
        Task<T> AddToCartAsync<T>(CartDto cartDto, string token = null);
        Task<T> UpdateCartAsync<T>(CartDto cartDto, string token = null);
        Task<T> RemoveFromCartAsync<T>(int cartId, string token = null);
    }
}