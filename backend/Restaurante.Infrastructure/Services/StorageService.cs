using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Services;

/// <summary>
/// File storage backed by Supabase Storage when configured, with a dev-only
/// local fallback that persists under Api/wwwroot/uploads (served via UseStaticFiles).
/// The local fallback is NOT production-safe: it is single-instance and lost on redeploy.
/// Configure "Supabase:Url" and "Supabase:ServiceRoleKey" to use remote storage.
/// </summary>
public class StorageService : IStorageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<StorageService> _logger;

    public StorageService(HttpClient httpClient, IConfiguration config, IWebHostEnvironment env, ILogger<StorageService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _env = env;
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        var url = _config["Supabase:Url"];
        var key = _config["Supabase:ServiceRoleKey"];
        var safeName = SanitizeFileName(fileName);

        var supabaseConfigured = !string.IsNullOrWhiteSpace(url)
            && !string.IsNullOrWhiteSpace(key)
            && !url.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
            && !key.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase);

        if (supabaseConfigured)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{url!.TrimEnd('/')}/storage/v1/object/{folder}/{safeName}");
            request.Headers.Add("apikey", key);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            request.Content = new StreamContent(fileStream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var keyValue = JsonSerializer.Deserialize<SupabaseUploadResponse>(json)?.Key;
            if (string.IsNullOrEmpty(keyValue))
                throw new InvalidOperationException("Supabase storage did not return a Key");

            return $"{url.TrimEnd('/')}/storage/v1/object/public/{keyValue}";
        }

        // Dev-only local fallback: uploads are stored under wwwroot/uploads and
        // served statically at /uploads/{folder}/{fileName}. See README (Phase 5).
        _logger.LogWarning("Supabase storage not configured — using dev-only local upload fallback");
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativePath = Path.Combine("uploads", folder, safeName);
        var fullPath = Path.Combine(webRoot, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using (var file = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(file);
        }

        return "/" + relativePath.Replace('\\', '/');
    }

    private static string SanitizeFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        var safeExt = new string(ext.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        if (safeExt.Length > 0)
            safeExt = "." + safeExt;

        return $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{safeExt}";
    }

    private sealed class SupabaseUploadResponse
    {
        public string? Key { get; set; }
    }
}
