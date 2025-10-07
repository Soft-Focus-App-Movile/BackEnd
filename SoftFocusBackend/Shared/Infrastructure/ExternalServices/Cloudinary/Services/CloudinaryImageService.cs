using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using Microsoft.Extensions.Options;

namespace SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

public class CloudinaryImageService : ICloudinaryImageService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryImageService> _logger;

    public CloudinaryImageService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryImageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!_settings.IsValid)
        {
            throw new InvalidOperationException("Cloudinary settings are not properly configured");
        }

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string folder)
    {
        try
        {
            var validation = ValidateImage(imageBytes, fileName);
            if (!validation.IsValid)
            {
                throw new ArgumentException(validation.ErrorMessage);
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var publicId = $"{folder}{fileNameWithoutExt}_{timestamp}";

            using var stream = new MemoryStream(imageBytes);
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                PublicId = publicId,
                Folder = folder,
                Transformation = new Transformation()
                    .Quality("auto:best")
                    .FetchFormat("auto")
                    .Width(1200)
                    .Height(1200)
                    .Crop("limit"),
                Overwrite = true,
                UseFilename = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
                throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");
            }

            _logger.LogInformation("Image uploaded successfully to Cloudinary: {PublicId}", publicId);
            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image to Cloudinary");
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Cannot delete image: publicId is null or empty");
                return false;
            }

            var deleteParams = new DeletionParams(publicId);
            var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

            var success = deleteResult.Result == "ok";
            
            if (success)
            {
                _logger.LogInformation("Image deleted successfully from Cloudinary: {PublicId}", publicId);
            }
            else
            {
                _logger.LogWarning("Cloudinary delete result: {Result} for {PublicId}", deleteResult.Result, publicId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image from Cloudinary: {PublicId}", publicId);
            return false;
        }
    }

    public string GetOptimizedImageUrl(string publicId, int width = 200, int height = 200)
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
                return string.Empty;

            var transformation = new Transformation()
                .Width(width)
                .Height(height)
                .Crop("fill")
                .Quality("auto")
                .FetchFormat("auto");

            return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate optimized URL for: {PublicId}", publicId);
            return string.Empty;
        }
    }

    public (bool IsValid, string ErrorMessage) ValidateImage(byte[] imageBytes, string fileName)
    {
        if (imageBytes.Length > _settings.MaxFileSizeBytes)
        {
            var maxSizeMB = _settings.MaxFileSizeBytes / (1024.0 * 1024.0);
            return (false, $"File size exceeds maximum allowed size of {maxSizeMB:F1}MB");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            var allowedExts = string.Join(", ", _settings.AllowedExtensions);
            return (false, $"File type {extension} not allowed. Allowed types: {allowedExts}");
        }

        if (!IsValidImageFormat(imageBytes))
        {
            return (false, "File does not appear to be a valid image");
        }

        return (true, string.Empty);
    }

    public string ExtractPublicIdFromUrl(string cloudinaryUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(cloudinaryUrl))
                return string.Empty;

            var uri = new Uri(cloudinaryUrl);
            var pathSegments = uri.AbsolutePath.Split('/');

            var uploadIndex = Array.IndexOf(pathSegments, "upload");
            if (uploadIndex == -1 || uploadIndex + 2 >= pathSegments.Length)
                return string.Empty;

            var startIndex = uploadIndex + 1;
            if (pathSegments[startIndex].StartsWith('v') && pathSegments[startIndex].Skip(1).All(char.IsDigit))
            {
                startIndex++;
            }

            var publicIdSegments = pathSegments[startIndex..];
            var lastSegment = publicIdSegments[^1];
            var lastSegmentWithoutExt = Path.GetFileNameWithoutExtension(lastSegment);
            publicIdSegments[^1] = lastSegmentWithoutExt;

            return string.Join("/", publicIdSegments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract public ID from URL: {Url}", cloudinaryUrl);
            return string.Empty;
        }
    }

    public async Task<string> UploadDocumentAsync(byte[] documentBytes, string fileName, string folder)
    {
        try
        {
            var validation = ValidateDocument(documentBytes, fileName);
            if (!validation.IsValid)
            {
                throw new ArgumentException(validation.ErrorMessage);
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var publicId = $"{folder}{fileNameWithoutExt}_{timestamp}";

            using var stream = new MemoryStream(documentBytes);

            // For PDFs and documents, use RawUploadParams instead of ImageUploadParams
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, stream),
                PublicId = publicId,
                Folder = folder,
                Overwrite = true,
                UseFilename = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary document upload failed: {Error}", uploadResult.Error.Message);
                throw new InvalidOperationException($"Document upload failed: {uploadResult.Error.Message}");
            }

            _logger.LogInformation("Document uploaded successfully to Cloudinary: {PublicId}", publicId);
            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload document to Cloudinary");
            throw;
        }
    }

    public (bool IsValid, string ErrorMessage) ValidateDocument(byte[] documentBytes, string fileName)
    {
        if (documentBytes.Length > _settings.MaxFileSizeBytes)
        {
            var maxSizeMB = _settings.MaxFileSizeBytes / (1024.0 * 1024.0);
            return (false, $"File size exceeds maximum allowed size of {maxSizeMB:F1}MB");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedDocumentExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

        if (!allowedDocumentExtensions.Contains(extension))
        {
            var allowedExts = string.Join(", ", allowedDocumentExtensions);
            return (false, $"File type {extension} not allowed. Allowed types: {allowedExts}");
        }

        // Validate file format by magic bytes
        if (extension == ".pdf")
        {
            if (!IsValidPdfFormat(documentBytes))
            {
                return (false, "File does not appear to be a valid PDF");
            }
        }
        else
        {
            if (!IsValidImageFormat(documentBytes))
            {
                return (false, "File does not appear to be a valid image");
            }
        }

        return (true, string.Empty);
    }

    private static bool IsValidImageFormat(byte[] imageBytes)
    {
        if (imageBytes.Length < 4)
            return false;

        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
            return true;

        if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
            return true;

        return false;
    }

    private static bool IsValidPdfFormat(byte[] pdfBytes)
    {
        if (pdfBytes.Length < 5)
            return false;

        // PDF files start with %PDF- (0x25 0x50 0x44 0x46 0x2D)
        return pdfBytes[0] == 0x25 &&
               pdfBytes[1] == 0x50 &&
               pdfBytes[2] == 0x44 &&
               pdfBytes[3] == 0x46 &&
               pdfBytes[4] == 0x2D;
    }
}