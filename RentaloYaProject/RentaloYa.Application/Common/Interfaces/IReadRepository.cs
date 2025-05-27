using System.Linq.Expressions;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IReadRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
