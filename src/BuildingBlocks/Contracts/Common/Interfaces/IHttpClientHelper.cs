namespace Contracts.Common.Interfaces;

public interface IHttpClientHelper
{
    Task<HttpResponseMessage> SendAsync(
        string path,
        HttpContent? content,
        HttpMethod method);
}