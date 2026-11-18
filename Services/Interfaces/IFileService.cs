using SwiftDrop.Helpers;

namespace SwiftDrop.Services.Interfaces
{
    public interface IFileService
    {

      Task<Result<string>> SavePublicImageAsync(IFormFile imageFile);
      Task<Result> DeletePublicImageAsync(string imageUrl);

    }
}
