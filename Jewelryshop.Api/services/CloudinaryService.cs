using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Jewelryshop.Api.Services;

public class CloudinaryService
{
    private readonly Cloudinary? _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
        {
            return;
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadProductImageAsync(IFormFile file)
    {
        if (_cloudinary is null)
        {
            throw new InvalidOperationException("Cloudinary settings are missing.");
        }

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "jewelry-shop/products",
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error is not null)
        {
            throw new InvalidOperationException(uploadResult.Error.Message);
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task DeleteImageAsync(string? imageUrl)
    {
        if (_cloudinary is null || string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var publicId = GetPublicIdFromUrl(imageUrl);
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
    }

    private static string? GetPublicIdFromUrl(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var uploadMarker = "/upload/";
        var uploadIndex = uri.AbsolutePath.IndexOf(uploadMarker, StringComparison.OrdinalIgnoreCase);
        if (uploadIndex < 0)
        {
            return null;
        }

        var publicPath = uri.AbsolutePath[(uploadIndex + uploadMarker.Length)..];
        var pathParts = publicPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (pathParts.Count == 0)
        {
            return null;
        }

        if (pathParts[0].StartsWith('v') && pathParts[0].Length > 1 && pathParts[0][1..].All(char.IsDigit))
        {
            pathParts.RemoveAt(0);
        }

        var publicIdWithExtension = string.Join('/', pathParts);
        var extension = Path.GetExtension(publicIdWithExtension);

        return string.IsNullOrWhiteSpace(extension)
            ? publicIdWithExtension
            : publicIdWithExtension[..^extension.Length];
    }
}
