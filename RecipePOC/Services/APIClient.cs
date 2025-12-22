using RecipePOC.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.UI.Notifications;

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

        public static async Task<List<RecipeDto>> GetAllRecipes(IHttpClientFactory _httpClientFactory)
        {
            var content = await GetRoute(_httpClientFactory, "api/recipes/list");

            if (string.IsNullOrWhiteSpace(content))
                return new List<RecipeDto>(); // default empty list

            return JsonSerializer.Deserialize<List<RecipeDto>>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<RecipeDto>();
        }

        /**
         * Ingredients
         * **/

        public static async Task<List<IngredientDto>> GetIngredients(IHttpClientFactory _httpClientFactory)
        {
            var content = await GetRoute(_httpClientFactory, "api/recipes/ingredients");

            if (string.IsNullOrWhiteSpace(content))
                return new List<IngredientDto>(); // default empty list

            return JsonSerializer.Deserialize<List<IngredientDto>>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<IngredientDto>();
        }

        public static async Task<string?> CreateIngredient(IHttpClientFactory _httpClientFactory, IngredientDto dto)
        {
            var content = await PostRoute<IngredientDto>(_httpClientFactory, "api/recipes/addingredient", dto);

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

        public static async Task<string?> CreateRecipe(IHttpClientFactory _httpClientFactory, RecipeDto dto)
        {
            var test = await PostRoute<RecipeDto>(_httpClientFactory, "api/recipes/addrecipe", dto);

            return test;
        }



        public static async Task<string?> CreateNotification(IHttpClientFactory httpClientFactory, NotificationDTO notificationDTO)
        {
            var test = await PostRoute<NotificationDTO>(httpClientFactory, "api/notification/create", notificationDTO); 

            return test; 
        }

        /** 
         * Users 
         * */ 
        public static async Task<string?> CompleteProfile(IHttpClientFactory _httpClientFactory, UserDTO request)
        {
            var test = await PostRoute<UserDTO>(_httpClientFactory, "api/auth/completeprofile", request);

            return test; 
        }

        public static async Task<string?> UpdateFollowingOrFollowers(IHttpClientFactory _httpClientFactory, UserDTO userDto, bool isFollowers)
        {
            if (isFollowers)
            {
                return null; 
            }

            return null; 
        }

        public static async Task<UserDTO> GetUser(IHttpClientFactory httpClientFactory, string user)
        {
            var content = await GetRoute(httpClientFactory, "api/Auth/GetUser", user, "userName");
            if (string.IsNullOrWhiteSpace(content))
                return new UserDTO(); // default empty list

            return JsonSerializer.Deserialize<UserDTO>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new UserDTO();
        }

        /**
         * 
         * Post Route 
         * */ 
        public static async Task<string?> PostRoute<T>(IHttpClientFactory _httpClientFactory, string apiRoute, T model)
        {
            var client = _httpClientFactory.CreateClient(AppConstants.HttpClientName);

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
                    query[param_key] = parameter_one;
                    //query[param_key] = param_key;
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
