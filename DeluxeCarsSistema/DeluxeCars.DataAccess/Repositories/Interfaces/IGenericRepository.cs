using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCars.DataAccess.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate);
        Task UpdateAsync(T entity);
        Task RemoveAsync(T entity); // Es preferible pasar la entidad para el tracking de EF
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> condition);
    }
}
