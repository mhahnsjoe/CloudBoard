using System.Linq.Expressions;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// Generic repository interface for common data access operations.
/// Demonstrates Interface Segregation - focused on CRUD basics.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets entity by primary key.
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<List<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Adds entity (tracked, not yet saved).
    /// </summary>
    void Add(T entity);

    /// <summary>
    /// Updates entity (tracked, not yet saved).
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Removes entity (tracked, not yet saved).
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Saves all tracked changes to database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
