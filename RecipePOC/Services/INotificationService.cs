using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services
{
    public interface INotificationService
    {
        Task<List<DB.Models.Notification>> GetNotifications(bool IsRead, string chefGuid); 
        Task<string?> AddNotification(string message, string userId);
        Task<bool> MarkRead(List<DB.Models.Notification> list); 
    }
}
