using RecipePOC.DTOs;
using SQLite;

namespace RecipePOC.Services
{
    public interface IAuthService
    {
        /***Get & Update User ***/
        Task<string> RegisterUser(SQLiteAsyncConnection _connection, LoginRequestDto userDTO); 
        Task<DB.Models.User> GetUser(SQLiteAsyncConnection _connection, string username);
        Task<string> UpdateBearerToken(string Token, string username, SQLiteAsyncConnection _connection);
        Task<string> UpdateTotalRecipeCount(string username, SQLiteAsyncConnection _connection);
        Task<string> UpdateUserProfile(SQLiteAsyncConnection _connection, string email,  string username, string bio, string language, string location, bool displayNotifs, string realName);

        Task<DB.Models.Social.Followers> GetFollowers(SQLiteAsyncConnection _connection, string usernameGuid);
        Task<DB.Models.Social.Following> GetFollowing(SQLiteAsyncConnection _connection, string usernameGuid);

        /** authentication **/
        Task<bool> IsUserAuthenticated();
        Task<string?> LoginAsync(LoginRequestDto dto);
        Task<string?> RegisterAsync(LoginRequestDto dto); 
        Task<string?> GetAuthenticatedUserAsync();
        Task<HttpClient> GetAuthenticatedHttpClientAsync();
        void Logout();
    }
}
