using HexMaster.Trunq.Resolver.Configuration;
using Microsoft.Extensions.Options;

namespace HexMaster.Trunq.Resolver.Services;

public interface IUrlBuilderService
{
    string BuildShortUrl(string shortCode);
}

public class UrlBuilderService : IUrlBuilderService
{
    private readonly TrunqOptions _options;

    public UrlBuilderService(IOptions<TrunqOptions> options)
    {
        _options = options.Value;
    }

    public string BuildShortUrl(string shortCode)
    {
        var baseUrl = _options.ShortUrlBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{shortCode}";
    }
}
