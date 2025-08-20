using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        if (string.IsNullOrWhiteSpace(shortCode) || 
            shortCode.Length < 4 || 
            shortCode.Length > 12 ||
            !shortCode.All(char.IsLetterOrDigit))
        {
            throw new ArgumentException("Short code must be 4-12 alphanumeric characters long.");
        }
        ShortCode = shortCode;
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