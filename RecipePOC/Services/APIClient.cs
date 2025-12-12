using RecipePOC.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipePOC.Services
{
    public class APIClient
    {
        /**
         * Recipes
         * **/
        public static async Task<string?> GetRecipeFavoritesAsync(IHttpClientFactory _httpClientFactory)
        {
            var content = await GetRoute(_httpClientFactory, "api/recipes/favoritelist"); 

            return content; 
        }

        public static async Task<string?> GetAllRecipes(IHttpClientFactory _httpClientFactory)
        {
            var content = await GetRoute(_httpClientFactory, "api/recipes/list");

            return content; 
        }

        /**
         * Ingredients
         * **/

        public static async Task<string?> GetIngredients(IHttpClientFactory _httpClientFactory)
        {
            var content = await GetRoute(_httpClientFactory, "api/recipes/ingredients");

            return content;
        }

        /**
         * Notifications
         * **/ 
        public static async Task<string?> GetNotifications(IHttpClientFactory _httpClientFactory, string userId, bool isRead)
        {
            if (!isRead)
            {
                var content = await GetRoute(_httpClientFactory, "api/notification/unread", userId, "user_id");

                return content;
            }
            else
            {
                var content = await GetRoute(_httpClientFactory, "api/notification/read", userId, "user_id");

                return content;
            }
        }

        public static async Task<string?> MarkRead(IHttpClientFactory _httpClientFactory, List<NotificationDTO> notifDtos)
        {
            // just pass one in for single
            var didDelete = await PostRoute<List<NotificationDTO>>(_httpClientFactory, "api/notification/markread", notifDtos); 

            return didDelete; 
        }

        public static async Task<string?> CreateNotification(IHttpClientFactory httpClientFactory, NotificationDTO notificationDTO)
        {
            var test = await PostRoute<NotificationDTO>(httpClientFactory, "api/notification/create", notificationDTO); 

            return test; 
        }

        /**
         * 
         * Post Route 
         * */ 
        public static async Task<string?> PostRoute<T>(IHttpClientFactory _httpClientFactory, string apiRoute, T model)
        {
            var client = _httpClientFactory.CreateClient();

            // Serialize the model to JSON
            var json = JsonSerializer.Serialize(model);

            // Prepare the content
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Send POST request
                var response = await client.PostAsync(apiRoute, content);

                // Optionally throw if not successful
                response.EnsureSuccessStatusCode();

                // Read and return response content
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostRoute: {ex.Message}");
                return null;
            }
        }

        /**
         * GetRoute
         * **/ 
        public static async Task<string?> GetRoute(IHttpClientFactory _httpClientFactory, string apiRoute, string? parameter_one = null, string? param_key = null)
        {
            var httpClient = _httpClientFactory.CreateClient(AppConstants.HttpClientName);
             
            if (!string.IsNullOrEmpty(parameter_one))
            {
                var uriBuilder = new UriBuilder(new Uri(httpClient.BaseAddress!, apiRoute));
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

                if (!string.IsNullOrEmpty(param_key))
                {
                    query[param_key] = param_key;
                }

                uriBuilder.Query = query.ToString();

                var response = await httpClient.GetAsync(uriBuilder.ToString());

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return string.Empty;
            }
            else
            {
                var response = await httpClient.GetAsync(apiRoute);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    return content;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
