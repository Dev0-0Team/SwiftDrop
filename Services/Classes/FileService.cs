using SwiftDrop.Helpers;
using SwiftDrop.Services.Interfaces;
using System.IO;

namespace SwiftDrop.Services.Classes
{
    public class FileService : IFileService
    {


        private readonly ICloudinaryService _cloudinaryService;
        public FileService(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }
        public async Task<Result<string>> SavePublicImageAsync(IFormFile imageFile)
        {
          

                if (imageFile == null || imageFile.Length == 0)
                    return new Result<string>(ResultStatus.ValidationError, null!, "There is no file");


                var extensionResult = ValidateImageExtension(imageFile);
                if (!extensionResult.IsSuccess)
                    return new Result<string>(extensionResult.StatusType, null!, extensionResult.Message!);

                var sizeResult = ValidateFileSize(imageFile, 3);
                if (!sizeResult.IsSuccess)
                    return new Result<string>(sizeResult.StatusType, null!, sizeResult.Message!);

                string? res = await _cloudinaryService.UploadImageAsync(imageFile);

                if (res == null)
                    return new Result<string>(ResultStatus.ValidationError, null!, "Failed to upload image");

                return new Result<string>(ResultStatus.Success, res, "The image has been saved successfully.");
         
        }


 


        public async Task<Result> DeletePublicImageAsync(string imageUrl)
        {
        
                if (string.IsNullOrWhiteSpace(imageUrl))
                    return new Result(ResultStatus.Success, "There is no image path to delete.");

                bool res = await _cloudinaryService.DeleteImageAsync(imageUrl);

                if (!res)
                    return new Result(ResultStatus.ValidationError, "Failed to delete image");

                return new Result(ResultStatus.Success, "The image has been deleted successfully.");
          
        }

        private Result ValidateImageExtension(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".tiff", ".oip" ,".svg" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                return new Result(ResultStatus.ValidationError, "The file type is not supported. Please select an image in a supported format.");

            return new Result(ResultStatus.Success);
        }

        private Result ValidateFileSize(IFormFile file , int maxSize)
        {
             int maxFileSize = maxSize * 1024 * 1024; 

            if (file.Length > maxFileSize)
                return new Result(ResultStatus.ValidationError, $"The file size is too large. The maximum allowed size is {maxSize} MB.");


            return new Result(ResultStatus.Success);
        }

 




    }
    }
