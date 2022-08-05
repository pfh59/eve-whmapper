using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WHMapper.Repositories
{
    public interface IDefaultRepository<T, U>
    {
        Task<IEnumerable<T>?> GetAll();
        Task<T?> GetById(U id);
        Task<T?> Create(T item);
        Task<T?> Update(U id, T item);
        Task<T?> DeleteById(U id);
    }
}
