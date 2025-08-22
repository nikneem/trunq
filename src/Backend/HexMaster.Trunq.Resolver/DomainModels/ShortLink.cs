using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexMaster.Trunq.Resolver.Validation;

namespace HexMaster.Trunq.Resolver.DomainModels;

public class ShortLink
{

    public Guid Id { get; }
    public string ShortCode { get; private set; }
    public string TargetUrl { get; private set; }
    public string SubjectId { get; private set; }
    public int? Hits { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void SetShortCode(string shortCode)
    {
        ShortCodeValidator.ValidateAndThrow(shortCode, nameof(shortCode));
        ShortCode = shortCode;
    }

    public void UpdateTargetUrl(string targetUrl)
    {
        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            throw new ArgumentException("Target URL must be a valid HTTP or HTTPS URL.", nameof(targetUrl));
        }
        TargetUrl = targetUrl;
    }

    public void IncrementHits(int count = 1)
    {
        Hits = (Hits ?? 0) + count;
    }

    private static string GenerateShortCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public ShortLink(Guid id, string shortCode, string targetUrl, string subjectId, DateTimeOffset createdAt)
    {
        Id = id;
        ShortCode = shortCode;
        TargetUrl = targetUrl;
        SubjectId = subjectId;
        CreatedAt = createdAt;
    }

    // Constructor with hits for deserialization from storage
    public ShortLink(Guid id, string shortCode, string targetUrl, string subjectId, DateTimeOffset createdAt, int? hits)
    {
        Id = id;
        ShortCode = shortCode;
        TargetUrl = targetUrl;
        SubjectId = subjectId;
        CreatedAt = createdAt;
        Hits = hits;
    }

    internal ShortLink(string targetUrl, string subjectId)
    {
        Id = Guid.NewGuid();
        ShortCode = GenerateShortCode();
        TargetUrl = targetUrl;
        SubjectId = subjectId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static ShortLink Create(string targetUrl, string subjectId)
    {
        return new ShortLink(targetUrl, subjectId);
    }


}