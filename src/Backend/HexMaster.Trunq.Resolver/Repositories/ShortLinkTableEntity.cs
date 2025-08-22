using Azure;
using Azure.Data.Tables;
using HexMaster.Trunq.Resolver.DomainModels;

namespace HexMaster.Trunq.Resolver.Repositories;

public class ShortLinkTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public Guid Id { get; set; }
    public string ShortCode { get; set; } = default!;
    public string TargetUrl { get; set; } = default!;
    public string SubjectId { get; set; } = default!;
    public int? Hits { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ShortLinkTableEntity()
    {
    }

    public ShortLinkTableEntity(ShortLink shortLink)
    {
        PartitionKey = "shortlink";
        RowKey = shortLink.ShortCode;
        Id = shortLink.Id;
        ShortCode = shortLink.ShortCode;
        TargetUrl = shortLink.TargetUrl;
        SubjectId = shortLink.SubjectId;
        Hits = shortLink.Hits;
        CreatedAt = shortLink.CreatedAt;
    }

    public ShortLink ToDomainModel()
    {
        return new ShortLink(Id, ShortCode, TargetUrl, SubjectId, CreatedAt, Hits);
    }
}