﻿using Digitec_Api.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Digitec_Api;
using Google.Cloud.Firestore.V1;

// This Class Library provides:
// 1. An interface to the database that stores the registered products
// 2. Classes (Tasks) that contain a specific functionality. For Example checking for changes in the price.
// 
// These classes are instantiated in the Daemon Project, which then creates a new Thread for every Task.
// The Task is supposed to run in the background, implementing a TaskInterface, that provides feedback and error logs.
namespace Digitec_Tools.Source
{
    public class Storage
    {
        private readonly FirestoreDb _database;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        private static Storage _instance;

        public static Storage GetInstance(AuthenticationStateProvider authenticationStateProvider)
        {
            return _instance ??= new Storage(authenticationStateProvider);
        }

        private Storage(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _database = FirestoreDb.Create("digitec-tools");
        }

        private async Task<DocumentReference> SetProduct(Product product)
        {
            if (product.ProductIdSimple.Equals(""))
                throw new Exception("Product contains no Id");

            var collection = _database.Collection("Products");
            var document = collection.Document(product.ProductIdSimple);

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                {"Name", product.Name},
                {"Brand", product.Brand},
                {"PriceCurrent", product.PriceCurrent},
                {"PriceOld", product.PriceOld},
                {"ProductIdSimple", product.ProductIdSimple},
                {"Url", product.Url}
            };

            await document.SetAsync(data);
            return await Task.FromResult(document);
        }
        
        public async Task<bool> AddNewProduct(Product product, UserData userData)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity.IsAuthenticated)
                return await Task.FromResult(false);

            try
            {
                var document = await SetProduct(product);

                var userCollection = document.Collection("Users");
                var userDocument = userCollection.Document(userData.Email);

                Dictionary<string, object> userDocData = new Dictionary<string, object>
                {
                    {"Email", userData.Email},
                    {"IPv4", userData.IPv4}
                };

                var result = await userDocument.SetAsync(userDocData);
                return result != null;
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<List<Product>> GetProductsForUser()
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authenticationState.User;
            if (!user.Identity.IsAuthenticated)
                return null;

            var res = new List<Product>();
            //Select from firestore where user collection contains user.Name

            var query = _database.CollectionGroup("Users").WhereEqualTo("Email", user.Identity.Name);
            var querySnapshot = await query.GetSnapshotAsync();
            var matchedUsers = querySnapshot.Documents.ToList();

            foreach (var matchedUser in matchedUsers)
            {
                var product = await matchedUser.Reference.Parent.Parent.GetSnapshotAsync();
                product.TryGetValue("ProductIdSimple", out string id);
                product.TryGetValue("Brand", out string brand);
                product.TryGetValue("Name", out string name);
                product.TryGetValue("Url", out string url);
                res.Add(new Product()
                {
                    ProductIdSimple = id,
                    Brand = brand,
                    Url = url,
                    Name = name
                });
            }

            return await Task.FromResult(res);
        }

        public async Task<bool> RemoveUserFromProduct(Product product)
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authenticationState.User;

            var productCollectionReference = _database.Collection("Products");
            var productReference = productCollectionReference.Document(product.ProductIdSimple);

            var userReference = productReference?.Collection("Users").Document(user.Identity.Name);

            if (userReference != null)
            {
                var result = await userReference?.DeleteAsync();
                return result != null;
            }

            return await Task.FromResult(false);
        }

        public async Task<List<Dictionary<string, object>>> GetUsersForProduct(string productSimpleId)
        {
            var userDocuments = await _database.Collection("Products").Document(productSimpleId).Collection("Users")
                .ListDocumentsAsync().ToArray();

            var users = new List<Dictionary<string, object>>();
            foreach (var user in userDocuments)
            {
                var snapshot = await user.GetSnapshotAsync();
                users.Add(snapshot.ToDictionary());
            }

            return users;
        }

        public async Task<List<Dictionary<string, object>>> GetAllProducts()
        {
            try
            {
                var snapshot = await _database.Collection("Products").GetSnapshotAsync();
                var productDocuments = snapshot.ToList();

                var products = new List<Dictionary<string, object>>();
                foreach (var item in productDocuments)
                {
                    products.Add(item.ToDictionary());
                }

                return await Task.FromResult(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task UpdateAllProducts(List<Dictionary<string, object>> products)
        {
            foreach (var product in products)
            {
                var api_res = await Digitec.GetProductInfo(product["Url"].ToString());
                if (!api_res.ProductIdSimple.Equals(product["ProductIdSimple"].ToString()))
                {
                    throw new Exception("Product Id's don't match! This is an API error.");
                }
                await SetProduct(api_res);
            }
        }
    }
}