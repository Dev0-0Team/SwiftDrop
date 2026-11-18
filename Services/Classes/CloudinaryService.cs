using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SwiftDrop.Helpers;
using SwiftDrop.Services.Interfaces;

namespace SwiftDrop.Services.Classes
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> options, ILogger<CloudinaryService> logger)
        {
            var settings = options.Value;

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }



        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty or null.");

                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
                    return null;
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError("Cloudinary upload failed: {Error}", ex.Message);
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string url)
        {
            try
            {
                var publicId = GetCloudinaryPublicId(url);

                if (string.IsNullOrWhiteSpace(publicId))
                {
                    _logger.LogWarning("Could not extract PublicId from URL: {Url}", url);
                    return false;
                }

                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Error != null) 
                {
                    _logger.LogError("Cloudinary deletion failed: {Error}", result.Error.Message);
                    return false;
                }

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError("Cloudinary deletion failed: {Error}", ex.Message);
                return false;
            }
        }

        private string? GetCloudinaryPublicId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex == -1 || uploadIndex + 1 >= segments.Length)
                    return null;

                var afterUpload = segments.Skip(uploadIndex + 1).ToArray();

                if (afterUpload.Length > 0 && afterUpload[0].StartsWith("v") &&
                    afterUpload[0].Length > 1 && afterUpload[0][1..].All(char.IsDigit))
                {
                    afterUpload = afterUpload.Skip(1).ToArray();
                }

                if (afterUpload.Length == 0)
                    return null;

                var publicIdWithPath = string.Join("/", afterUpload);

                var lastDotIndex = publicIdWithPath.LastIndexOf('.');
                if (lastDotIndex > 0)
                    publicIdWithPath = publicIdWithPath[..lastDotIndex];

                return publicIdWithPath;
            }
            catch
            {
                return null;
            }
        }
    }
}