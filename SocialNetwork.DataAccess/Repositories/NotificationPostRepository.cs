using SocialNetwork.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DataAccess.Repositories
{
    public class NotificationPostRepository : INotificationPostRepository
    {
        public readonly SocialNetworkdDataContext _context;

        public NotificationPostRepository(SocialNetworkdDataContext context)
        {
            _context = context;
        }

        public async Task CreateImageNotificationAsync(string userId,string postId)
        {
            var notification = new NotificationEntity
            {
                Id = Guid.NewGuid().ToString(),
                Messeage = "Ảnh trong bài viết của bạn bị phát hiện là nhạy cảm. Bài viết không được đăng.",
                ReceiverId = userId,
                SenderId = userId,
                Type = 3, // 3 là notification nhạy cảm
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsRead = false,
                IsDelete = false
            };

            await _context.AddAsync(notification);
            await _context.SaveChangesAsync();

            //var exists = await _context.Notifications.AnyAsync(n =>
            //     n.ReceiverId == userId &&
            //      n.Type == 3 &&
            //      n.IsDelete == false &&
            //      n.IsRead == false &&

            //         n.Messeage == $"Bài viết đã bị chặn.");

            //if (!exists)
            //{
            //    var notification = new NotificationEntity
            //    {
            //        Id = Guid.NewGuid().ToString(),
            //        Messeage = $"Bài viết đã bị chặn.",
            //        ReceiverId = userId,
            //        SenderId = userId,
            //        Type = 3,
            //        CreatedAt = DateTime.UtcNow,
            //        UpdatedAt = DateTime.UtcNow,
            //        IsRead = false,
            //        IsDelete = false
            //    };

            //    await _context.AddAsync(notification);
            //    await _context.SaveChangesAsync();

            //}

        }



        public async Task CreateNotificationAsync(IEnumerable<NotificationEntity> notification)
        {
            //var user = await _context.Users.FindAsync(notification.UserId);
            try
            {
                //notification.User = user;
                await _context.Notifications.AddRangeAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving notification: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsDelete = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<NotificationEntity>> GetNotificationByUserAsync(string userId)
        {

            return await _context.Notifications
                .Where(n => n.ReceiverId == userId && !n.IsDelete)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        }

        public async Task MakeAsReadAsync(string id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                //notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
