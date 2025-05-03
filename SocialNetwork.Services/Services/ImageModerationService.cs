//using Google.Apis.Auth.OAuth2;
//using Google.Cloud.Vision.V1;
//using Grpc.Auth;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Channels;
//using System.Threading.Tasks;

//namespace SocialNetwork.Services.Services
//{
//    public class ImageModerationService:IImageModerationService
//    {
//        private readonly ImageAnnotatorClient _client;

//        public ImageModerationService()
//        {
//            try
//            {
//                var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "", "testwebsocialnetword-6d49863a606e.json");

//                var credential = GoogleCredential.FromFile(jsonPath);

//                var builder = new ImageAnnotatorClientBuilder
//                {
//                    Credential = credential
//                };

//                _client = builder.Build();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Lỗi khởi tạo Google Vision: " + ex.Message);
//                throw;
//            }
//        }

//        public async Task<bool> IsImageSafeAsync(string imageUrl)
//        {
//            try
//            {
//                var imageBytes = await DownloadImageAsync(imageUrl);
//                var image = Google.Cloud.Vision.V1.Image.FromBytes(imageBytes);
//                //var result = await _client.DetectSafeSearchAsync(image);
//                var annotation = await _client.DetectSafeSearchAsync(image);


//                //var annotation = result.SafeSearchAnnotation;

//                bool isUnsafe = annotation.Adult == Likelihood.Likely || annotation.Adult == Likelihood.VeryLikely
//                             || annotation.Racy == Likelihood.Likely || annotation.Racy == Likelihood.VeryLikely
//                             || annotation.Violence == Likelihood.Likely || annotation.Violence == Likelihood.VeryLikely;

//                return !isUnsafe;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Lỗi kiểm duyệt ảnh: " + ex.Message);
//                return false;
//            }
//        }
//        private async Task<byte[]> DownloadImageAsync(string imageUrl)
//        {
//            using (var httpClient = new HttpClient())
//            {
//                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
//                return imageBytes;
//            }
//        }
//    }

//}