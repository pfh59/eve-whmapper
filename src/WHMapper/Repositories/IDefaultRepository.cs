﻿namespace WHMapper.Repositories
{
    public interface IDefaultRepository<T, U>
    {
        Task<IEnumerable<T>?> GetAll();
        Task<T?> GetById(U id);
        Task<T?> Create(T item);
        Task<T?> Update(U id, T item);
        Task<bool> DeleteById(U id);
    }
}
