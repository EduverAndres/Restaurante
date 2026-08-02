using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Restaurante.Infrastructure.Services;

namespace Restaurante.Tests;

/// <summary>
/// Verifies the real webhook signature code in PaymentService
/// (HMAC-SHA256 over the raw body with Wompi:WebhookSecret).
/// </summary>
public class PaymentSignatureTests
{
    private const string Secret = "whsec_demo_1234567890abcdef";

    private static PaymentService BuildService(string? secret)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Wompi:WebhookSecret"] = secret ?? string.Empty,
                ["PaymentProvider:Mode"] = "Mock",
            })
            .Build();

        return new PaymentService(config, new HttpClient());
    }

    private static string Sign(string body, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
    }

    private const string RawBody = """
        {"event":"transaction.updated","data":{"id":"txn_01","reference":"rest-abc","status":"APPROVED","amount_in_cents":25700}}
        """;

    [Fact]
    public async Task ValidSignature_IsAccepted()
    {
        var service = BuildService(Secret);
        var signature = Sign(RawBody, Secret);

        Assert.True(await service.VerifyWebhookSignatureAsync(RawBody, signature));
    }

    [Fact]
    public async Task TamperedBody_IsRejected()
    {
        var service = BuildService(Secret);
        var signature = Sign(RawBody, Secret);

        var tampered = RawBody.Replace("APPROVED", "DECLINED");
        Assert.False(await service.VerifyWebhookSignatureAsync(tampered, signature));
    }

    [Fact]
    public async Task WrongSecretSignature_IsRejected()
    {
        var service = BuildService(Secret);

        var signature = Sign(RawBody, "another-secret");
        Assert.False(await service.VerifyWebhookSignatureAsync(RawBody, signature));
    }

    [Fact]
    public async Task EmptySignature_IsRejected()
    {
        var service = BuildService(Secret);

        Assert.False(await service.VerifyWebhookSignatureAsync(RawBody, string.Empty));
        Assert.False(await service.VerifyWebhookSignatureAsync(RawBody, "   "));
    }

    [Fact]
    public async Task WrongLengthSignature_IsRejected()
    {
        var service = BuildService(Secret);
        var signature = Sign(RawBody, Secret);

        Assert.False(await service.VerifyWebhookSignatureAsync(RawBody, signature[..^1]));
    }

    [Fact]
    public async Task SignatureComparison_IsCaseInsensitive()
    {
        var service = BuildService(Secret);
        var signature = Sign(RawBody, Secret).ToUpperInvariant();

        Assert.True(await service.VerifyWebhookSignatureAsync(RawBody, signature));
    }

    [Fact]
    public async Task UnconfiguredSecret_Throws()
    {
        var service = BuildService(null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.VerifyWebhookSignatureAsync(RawBody, "anything"));
    }

    [Fact]
    public async Task ChangeMeSecret_Throws()
    {
        var service = BuildService("CHANGE_ME");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.VerifyWebhookSignatureAsync(RawBody, "anything"));
    }
}
