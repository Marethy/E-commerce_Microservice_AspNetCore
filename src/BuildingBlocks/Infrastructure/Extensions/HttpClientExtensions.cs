using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Extensions;

public static class HttpClientExtensions
{
    public static async Task<T> ReadContentAs<T>(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");
        }

        var dataAsString = await response.Content.ReadAsStringAsync();

        var data = JsonSerializer.Deserialize<T>(dataAsString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReferenceHandler = ReferenceHandler.Preserve });

        return data;
    }

    public static async Task<string> ReadContentAs(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");
        }

        var dataAsString = await response.Content.ReadAsStringAsync();

        return dataAsString;
    }
}