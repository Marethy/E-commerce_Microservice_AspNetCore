using Contracts.Common.Interfaces;

namespace Infrastructure.Common;

public class HttpClientHelper(HttpClient httpClient) : IHttpClientHelper
{
    public async Task<HttpResponseMessage> SendAsync(string path, HttpContent content, HttpMethod method)
    {
        var httpRequest = new HttpRequestMessage()
        {
            RequestUri = new Uri(path),
            Content = content,
            Method = method
        };

        //string token = await _tokenManager.GetTokenAsync(apiResource.Scopes);

        //if (!string.IsNullOrEmpty(token))
        //{
        //    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //}

        return await httpClient.SendAsync(httpRequest);
    }
}