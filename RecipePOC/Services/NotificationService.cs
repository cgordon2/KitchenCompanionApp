using RecipePOC.DB;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private SQLiteAsyncConnection _connection; 

        public NotificationService(IHttpClientFactory httpClientFactory)
        {
            _connection = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> MarkRead(List<DB.Models.Notification> notifications)
        {
            await _connection.UpdateAllAsync(notifications);

            return true; 
        }

        public async Task<List<DB.Models.Notification>> GetNotifications(bool IsRead, string chefGuid)
        {
            if (!IsRead)
            {
                var unreadNotifs = await _connection.Table<DB.Models.Notification>().Where(r => r.IsRead == false).Where(r => r.ChefGUID == chefGuid).ToListAsync();
                return unreadNotifs;
            }
            else
            {
                var readNotifs = await _connection.Table<DB.Models.Notification>().Where(r => r.IsRead == true).Where(r => r.ChefGUID == chefGuid).ToListAsync();

                return readNotifs;
            }  
        }

        /*public async Task<string?> GetNotifications(bool IsRead)
        {
            if (!IsRead)
            {
                var notifications = await APIClient.GetNotifications(_httpClientFactory, "8", false);

                return notifications; 
            }
            else
            {
                var notifications = await APIClient.GetNotifications(_httpClientFactory, "8", true); 

                return notifications; 
            } 
        }**/

        public async Task<string?> AddNotification(string message, string userId)
        {
            return string.Empty; 
        }
    }
}
