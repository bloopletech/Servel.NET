namespace Servel.NET.Db;

public interface ITimestampedModel<T> : IModel<T> where T : ITimestampedModel<T>, IModel<T>
{
    long CreatedAt { get; set; }
    long UpdatedAt { get; set; }
}
