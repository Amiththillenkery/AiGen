using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// Create readmd file
/// As a user, I want to create readmd file so that I can improve my workflow and productivity. This should work with the existing .NET / C#, .NET Solution, ASP.NET, .NET Config stack in Amiththillenkery/AiGen.
/// </summary>
public class CreateReadmdFileDto
{
    public Guid Id { get; set; }
    // AC1: Given the feature is implemented, when a user interacts with it, then it performs the expected action
    // AC2: Edge cases and error states are handled gracefully
    // AC3: The feature is accessible and works on mobile devices
    // AC4: Compatible with existing C# codebase patterns
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateCreateReadmdFileRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateCreateReadmdFileRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    public string? Description { get; set; }
}
