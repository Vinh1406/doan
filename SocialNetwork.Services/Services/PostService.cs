using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain;
using SocialNetwork.DTOs.Response;
using SocialNetwork.Services.IServices;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace SocialNetwork.Services.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IBaseRepository<ImagesOfPostEntity> _imageRepository;
        private readonly INotificationPostService _notificationservice;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IPostHubService _postHubService;
        private readonly IImageModerationService _imageModerationService;
        public PostService(IPostRepository postRepository,
            UserManager<UserEntity> userManager, IUserRepository userRepository
            , IBaseRepository<ImagesOfPostEntity> imageRepository, IMapper mapper, INotificationPostService notificationservice, IRelationshipRepository relationshipRepository, IPostHubService postHubService, IImageModerationService imageModerationService = null)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _imageRepository = imageRepository;
            _mapper = mapper;
            _notificationservice = notificationservice;
            _relationshipRepository = relationshipRepository;
            _postHubService = postHubService;
            _imageModerationService = imageModerationService;
        }




        public async Task<PostResponse> CreatePostAsync(PostRequest postRequest, string userID)
        {

            try
            {


                var postEntity = _mapper.Map<PostEntity>(postRequest);
                postEntity.PostID = Guid.NewGuid().ToString();
                postEntity.UserID = userID;
                postEntity.User = await _userRepository.GetByIDAsync(userID);
                if (postEntity.User == null)
                {
                    throw new Exception("User not found.");
                }
                if (postRequest.Images != null && postRequest.Images.Count > 0)
                {
                    var imageModeration = new ImageModerationService();
                    foreach (var image in postRequest.Images)
                    {
                        if (string.IsNullOrWhiteSpace(image.ImgUrl))
                        {
                            throw new ArgumentException("Image URL cannot be null or empty.");
                        }
                        var imageIsSafe = await imageModeration.IsImageSafeAsync(image.ImgUrl);
                        if (!imageIsSafe)
                        {

                            //await _notificationservice.CreateSensitiveImageNotificationAsync(userID,postEntity.PostID);

                            // Gửi thông báo qua SignalR đến danh sách bạn bè
                            //await _postHubService.SendNotificationToMultipleUsers(friendToNotify, notification);
                            var userImage = new FriendViewModel
                            {
                                AvatarUrl = postEntity.User.AvatarUrl,
                                LastName = postEntity.User.LastName,
                                FirstName = postEntity.User.FirstName,

                            };

                            await _postHubService.CreateImageNotificationAsync(userID, userImage);


                            throw new Exception("Image contains unsafe content.");
                        }

                        var imageEntity = _mapper.Map<ImagesOfPostEntity>(image);

                        imageEntity.PostID = postEntity.PostID;

                    }
                }

                //Console.WriteLine($"Creating Post - PostID: {postEntity.PostID}, Content: {postEntity.Content}");
                await _postRepository.AddAsync(postEntity);
                await _postRepository.SaveChangeAsync();

                var postResponse = _mapper.Map<PostResponse>(postEntity);
                postResponse.FirstName = postEntity.User?.FirstName;
                postResponse.LastName = postEntity.User?.LastName;
                postRequest.Images = postResponse.Images;

                return postResponse;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Update Error: {dbEx.InnerException?.Message}");
                throw new Exception("An error occurred while creating the post.", dbEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                throw new Exception("An error occurred while creating the post.", ex);
            }
        }



        //////Friends
        ////var friends = await _relationshipRepository.GetFriendIdByUserId(userID);

        ////var friendToNotify = friends.Where(x => x != userID).ToList();

        ////if (friends.Any())
        ////{
        ////    // Nội dung thông báo
        ////    var content = $"{postEntity.User.FirstName} {postEntity.User.LastName} vừa đăng một bài viết mới.";


        ////    var notification = new NotificationPostViewModel
        ////    {
        ////        Content = content,
        ////        Type = "New_Post",
        ////        UserId = userID,
        ////        //friendId= friendToNotify,
        ////        //PostId = postEntity.PostID,
        ////        //CreatedAt = DateTime.UtcNow
        ////    };


        ////    // Tạo thông báo trong cơ sở dữ liệu
        ////    await _notificationservice.CreateNotificationAsync(notification, friendToNotify);

        ////    // Gửi thông báo qua SignalR đến danh sách bạn bè
        ////    //await _postHubService.SendNotificationToMultipleUsers(friendToNotify, notification);
        ////}

        ////await _postHubService.SendPostAsync(postResponse);


        ////var content = $"{postEntity.User.FirstName} {postEntity.User.LastName}  BÀI VIẾT CỦA BẠN CHỨA NHỮNG ẢNH ĐỘC HẠI. BÀI VIẾT KHÔNG ĐƯỢC ĐĂNG";


        ////var notification = new NotificationPostViewModel
        ////{
        ////    Content = content,
        ////    Type = "New_Post",
        ////    UserId = userID,
        ////};




        //// Tạo thông báo trong cơ sở dữ liệu
        //await _notificationservice.CreateSensitiveImageNotificationAsync(userID);

        //// Gửi thông báo qua SignalR đến danh sách bạn bè
        ////await _postHubService.SendNotificationToMultipleUsers(friendToNotify, notification);


        //await _postHubService.CreateImageNotificationAsync(userID);


        public async Task<bool> DeletePostAsync(string postId)
        {
            await _postRepository.Delete(postId);
            await _postHubService.SendRefusePostAsync(postId);
            return true;
        }



        public async Task<PageResult<PostViewModel>> GetAllPostsAsync(string userId, int pageIndex, int pageSize)
        {

            var posts = await _postRepository.GetAllAsync(userId);

            var total = posts.Count();
            //var pagePost = posts.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var pagePost = posts.ToList();
            var getPost = new PageResult<PostViewModel>
            {
                CurrentPage = pageIndex,
                TotalCount = total,
                Data = pagePost,
            };
            return getPost;

        }

        public async Task<PostViewModel> GetPostByIdAsync(string postId)
        {
            var post = await _postRepository.GetByIDAsync(postId);
            if (post == null)
            {
                return null;
            }
            return _mapper.Map<PostViewModel>(post);
        }


        //public async Task<PostViewModel> UpdatePostAsync(PostViewModel post)
        //{
        //    var postEntity = await _postRepository.GetByIDAsync(post.PostID);
        //    if (postEntity == null)
        //    {
        //        throw new Exception("Không tồn tại bài viết");
        //    }
        //    _mapper.Map(post, postEntity);
        //    _postRepository.Update(postEntity);
        //    await _postRepository.SaveChangeAsync();
        //    return _mapper.Map<PostViewModel>(postEntity);
        //}

        public async Task<PageResult<PostViewModel>> GetPostsByUserIdAsync(string userId, int pageSize, int pageIndex)
        {
            var posts = await _postRepository.GetAllAsync(userId);
            var userPosts = posts.Where(p => p.UserID == userId).ToList(); ;
            var total = userPosts.Count();
            var pagePost = posts.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var getPost = new PageResult<PostViewModel>
            {
                CurrentPage = pageIndex,
                TotalCount = total,
                Data = userPosts,
            };
            return getPost;
        }

        public Task<IEnumerable<AdminBrowsePostViewModel>> GetAdminBrowseAsync()
        {
            var postWait = _postRepository.GetAdminBrowseAsync();
            return postWait;
        }
    }
}
