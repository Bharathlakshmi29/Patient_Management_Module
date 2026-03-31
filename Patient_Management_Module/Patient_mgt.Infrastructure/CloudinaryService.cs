using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace Patient_mgt.Infrastructure
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || cloudName.Contains("your-") ||
                string.IsNullOrEmpty(apiKey) || apiKey.Contains("your-") ||
                string.IsNullOrEmpty(apiSecret) || apiSecret.Contains("your-"))
            {
                throw new InvalidOperationException("Cloudinary credentials are not properly configured. Please update appsettings.json with your actual Cloudinary credentials.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(byte[] imageData, string fileName, string folder = "patient-photos")
        {
            using var stream = new MemoryStream(imageData);
            
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, stream),
                Folder = folder,
                PublicId = $"{folder}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}",
                Overwrite = true
            };

            Console.WriteLine($"Cloudinary upload params - Folder: {folder}, PublicId: {uploadParams.PublicId}");

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.Error != null)
            {
                Console.WriteLine($"Cloudinary upload error: {uploadResult.Error.Message}");
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            Console.WriteLine($"Cloudinary upload successful. SecureUrl: {uploadResult.SecureUrl}");
            return uploadResult.SecureUrl.ToString();
        }

        public async Task<string> UploadDocumentAsync(byte[] documentData, string fileName, string folder = "medical-reports")
        {
            using var stream = new MemoryStream(documentData);
            
            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(fileName, stream),
                Folder = folder,
                PublicId = Guid.NewGuid().ToString(),  // ⭐ NO folder here - avoid double mapping
                Overwrite = true,
                Type = "upload",  // ⭐ VERY IMPORTANT - makes file public
                AccessMode = "public"  // ⭐ Extra safety - explicitly public
            };
            
            Console.WriteLine($"Uploading document with params - Folder: {folder}, PublicId: {uploadParams.PublicId}, Type: {uploadParams.Type}");

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            Console.WriteLine($"Document upload successful. SecureUrl: {uploadResult.SecureUrl}");
            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }

        public Task<string> GetSignedUrlAsync(string publicId)
        {
            // For now, return null - signed URLs require more complex implementation
            // The main fix is making new uploads public with Type = "upload"
            return Task.FromResult<string>(null);
        }

        public async Task<byte[]> DownloadFileAsync(string url)
        {
            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(url);
        }
    }
}