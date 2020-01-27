﻿using System.Threading.Tasks;
using Shopping_Tools_Api_Services.Models;

namespace Shopping_Tools_Api_Services.Core.Digitec
{
    public interface IApi
    {
        public string OnlineShopName { get; }
        public Task<Product> GetProductInfo(string productUrl);
        public bool IsValidUrl(string url);
    }
}