namespace SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

public interface ICloudinaryImageService
{
    Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string folder);
    Task<bool> DeleteImageAsync(string publicId);
    string GetOptimizedImageUrl(string publicId, int width = 200, int height = 200);
    (bool IsValid, string ErrorMessage) ValidateImage(byte[] imageBytes, string fileName);
    string ExtractPublicIdFromUrl(string cloudinaryUrl);
}