using Microsoft.EntityFrameworkCore;

namespace Mercury.Models.Jobs;

[Index(nameof(Id), IsUnique = true)]
public class Job
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string JobType { get; init; }
    public required string Schedule { get; init; }
    public string? Description { get; init; } = string.Empty;
    public bool Active { get; init; } = true;
}
