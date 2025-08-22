using System.ComponentModel.DataAnnotations;
using HexMaster.Trunq.Resolver.Validation;

namespace HexMaster.Trunq.Resolver.Models;

public record CreateLinkRequest
{
    [Required]
    [Url]
    public required string TargetUrl { get; init; }
}

public record CreateLinkResponse
{
    public required Guid Id { get; init; }
    public required string ShortCode { get; init; }
    public required string TargetUrl { get; init; }
    public required string ShortUrl { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public record UpdateLinkRequest
{
    [Required]
    [Url]
    public required string TargetUrl { get; init; }
    
    [ValidShortCode]
    public string? ShortCode { get; init; }
}