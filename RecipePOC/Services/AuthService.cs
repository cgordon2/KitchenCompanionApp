using RecipePOC.DB;
using RecipePOC.DTOs;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipePOC.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory; 

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> RegisterUser(SQLiteAsyncConnection _connection, LoginRequestDto userDTO)
        {
            var error = await this.RegisterAsync(userDTO);

            if (!string.IsNullOrEmpty(error))
            {
            }
            
            return "Couldn't register, unknown reason"; 
        }


        public Task<DB.Models.Social.Followers> GetFollowers(SQLiteAsyncConnection _connection, string usernameGuid)
        {
            return null; 
        }
        public Task<DB.Models.Social.Following> GetFollowing(SQLiteAsyncConnection _connection, string usernameGuid)
        {
            return null; 
        }

        public async Task<string> UpdateBearerToken(string Token, string username, SQLiteAsyncConnection _connection)
        {

            var user = await _connection.Table<DB.Models.User>().Where(r => r.UserName == username).FirstOrDefaultAsync();

            if (user != null)
            {
                user.BearerToken = Token;
                await _connection.UpdateAsync(user);

                return "Update success";
            }

            return "Update failure"; 
        }

        public async Task<string> UpdateUserProfile(SQLiteAsyncConnection _connection, string email,  string username, string bio, string language, string location, bool displayNotifs)
        {
            var user = await _connection.Table<DB.Models.User>().Where(r => r.UserName == username).FirstOrDefaultAsync();

            if (user != null)
            {
                user.ShortBio = bio;
                user.Language = language;
                user.DisplayNotifications = displayNotifs;
                await _connection.UpdateAsync(user);

                return "Update success";
            }

            return "Update failure";
        }

        public async Task<string> UpdateTotalRecipeCount(string username, SQLiteAsyncConnection _connection)
        {
            var user = await _connection.Table<DB.Models.User>().Where(r => r.UserName == username).FirstOrDefaultAsync();

            if (user != null)
            {
                var localRecipesAmount = user.TotalRecipes;
                user.TotalRecipes = localRecipesAmount + 1;

                await _connection.UpdateAsync(user);

                return "Update success";
            }

            return "Update failure";
        }

        public async Task<DB.Models.User> GetUser(SQLiteAsyncConnection _connection, string username)
        {
            var user = await _connection.Table<DB.Models.User>().Where(r => r.UserName == username).FirstOrDefaultAsync(); 

            return user; 
        }

        public async Task<bool> IsUserAuthenticated()
        {
            var serializedData = await SecureStorage.Default.GetAsync(AppConstants.AuthStorageKeyName);
            return !string.IsNullOrWhiteSpace(serializedData);
        }

        public async Task<string?> GetAuthenticatedUserAsync()
        {
            var serializedData = await SecureStorage.Default.GetAsync(AppConstants.AuthStorageKeyName);
            if (!string.IsNullOrWhiteSpace(serializedData))
            {
                return serializedData;
            }

            return null;
        }

        public async Task<string?> RegisterAsync(LoginRequestDto dto)
        {
            var httpClient = _httpClientFactory.CreateClient(AppConstants.HttpClientName);

            var response = await httpClient.PostAsJsonAsync<LoginRequestDto>("api/auth/register", dto);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var serializedData = content;
                await SecureStorage.Default.SetAsync(AppConstants.AuthStorageKeyName, serializedData);

                return content;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<string?> LoginAsync(LoginRequestDto dto)
        {
            var httpClient = _httpClientFactory.CreateClient(AppConstants.HttpClientName);

            var response = await httpClient.PostAsJsonAsync<LoginRequestDto>("api/auth/login", dto);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var serializedData = content; 
                await SecureStorage.Default.SetAsync(AppConstants.AuthStorageKeyName, serializedData);

                return content; 
            }
            else
            {
                return string.Empty;
            }
        }

        public void Logout() { }

        public async Task<HttpClient> GetAuthenticatedHttpClientAsync()
        {
            var httpClient = _httpClientFactory.CreateClient(AppConstants.HttpClientName);
            var authenticatedUserToken = await GetAuthenticatedUserAsync();

            httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", authenticatedUserToken);

            return httpClient; 
        }
    }
}
