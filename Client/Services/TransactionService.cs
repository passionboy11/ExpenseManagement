using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using static Client.DTO.TransactionDto; // adjust namespace to match DTO location

public class TransactionService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _nav;

    public TransactionService(HttpClient httpClient, NavigationManager nav)
    {
        _httpClient = httpClient;
        _nav = nav;
    }

    public async Task<List<TransactionResponse>> GetTransactionsAsync()
    {
        try
        {
            var res = await _httpClient.GetAsync("Transaction/readtransaction");
            if (!res.IsSuccessStatusCode)
                return new List<TransactionResponse>();

            var json = await res.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return JsonSerializer.Deserialize<List<TransactionResponse>>(doc.RootElement.GetProperty("Data"));
        }
        catch
        {
            return new List<TransactionResponse>();
        }
    }

    public async Task<bool> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var res = await _httpClient.PostAsJsonAsync("Transaction/createtransaction", request);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> EditTransactionAsync(EditTransactionRequest request)
    {
        var res = await _httpClient.PutAsJsonAsync("Transaction/edittransaction", request);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTransactionAsync(DeleteTransactionRequest request)
    {
        var res = await _httpClient.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(_httpClient.BaseAddress + "Transaction/deletetransaction"),
            Content = JsonContent.Create(request)
        });

        return res.IsSuccessStatusCode;
    }

    public async Task<decimal> GetBalanceAsync()
    {
        var res = await _httpClient.GetAsync("Transaction/getbalance");
        if (!res.IsSuccessStatusCode)
            return 0;

        var json = await res.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("Data").GetDecimal();
    }
}
