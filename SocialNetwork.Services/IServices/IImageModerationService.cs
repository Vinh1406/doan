using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Services.IServices
{
    public interface IImageModerationService
    {
        Task<bool> IsImageSafeAsync(string imageUrl);
    }
}
