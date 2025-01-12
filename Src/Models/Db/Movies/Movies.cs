using System.ComponentModel.DataAnnotations;

namespace Mercury.Models.Db;

public class Movie
{
    [Key]
    public required Guid Id { get; init; }

    public required string Title { get; set; }

    public required int GenreId { get; set; }
    public Genre? Genre { get; set; }
}
