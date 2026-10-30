namespace Servel.NET.Db;

public interface ITimestampedModel
{
    long CreatedAt { get; set; }
    long UpdatedAt { get; set; }
}
