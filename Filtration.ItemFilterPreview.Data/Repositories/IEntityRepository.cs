using System;
using System.Linq;
using System.Linq.Expressions;

namespace Filtration.ItemFilterPreview.Data.Repositories
{
    public interface IEntityRepository<T> : IDisposable
    {
        IQueryable<T> All { get; }
        IQueryable<T> AllIncluding(params Expression<Func<T, object>>[] includeProperties);
        T Find(int id);
        void InsertOrUpdate(T itemSet);
        void Delete(int id);
        void Save();
    }
}