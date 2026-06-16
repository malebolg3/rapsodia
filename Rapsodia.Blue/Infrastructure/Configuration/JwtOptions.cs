using System.ComponentModel.DataAnnotations;

namespace Rapsodia.Blue.Infrastructure.Configuration;

public class JwtOptions
{
    [Required]
    [MinLength(32)]
    public string Key { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;
}