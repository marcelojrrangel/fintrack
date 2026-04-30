using System.Net;

namespace FinTrack.Backend.IntegrationTests.Support;

public class TestContext
{
    public HttpClient ApiClient { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public HttpResponseMessage? LastResponse { get; set; }
    public Dictionary<string, string> TransactionIds { get; set; } = new();
    public Dictionary<string, object> TestData { get; set; } = new();
    public string? LastTransactionId { get; set; }
    public string? LastCategoryId { get; set; }

    public void StoreTransactionId(string key, string id)
    {
        TransactionIds[key] = id;
        LastTransactionId = id;
    }

    public string GetTransactionId(string key)
    {
        return TransactionIds.TryGetValue(key, out var id) ? id : string.Empty;
    }

    public void AssertLastResponseSuccess()
    {
        if (LastResponse == null)
            throw new InvalidOperationException("Nenhuma resposta HTTP foi registrada");

        if (!LastResponse.IsSuccessStatusCode)
        {
            var content = LastResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            throw new InvalidOperationException(
                $"Esperava sucesso mas recebeu {LastResponse.StatusCode}. Conteúdo: {content}");
        }
    }

    public void AssertLastResponseStatusCode(HttpStatusCode expectedStatus)
    {
        if (LastResponse == null)
            throw new InvalidOperationException("Nenhuma resposta HTTP foi registrada");

        if (LastResponse.StatusCode != expectedStatus)
        {
            var content = LastResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            throw new InvalidOperationException(
                $"Esperava {expectedStatus} mas recebeu {LastResponse.StatusCode}. Conteúdo: {content}");
        }
    }
}
