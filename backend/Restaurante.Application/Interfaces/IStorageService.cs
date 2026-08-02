namespace Restaurante.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder);
}
