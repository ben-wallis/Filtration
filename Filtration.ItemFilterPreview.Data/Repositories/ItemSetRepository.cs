using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Filtration.ItemFilterPreview.Data.DataContexts;
using Filtration.ObjectModel;

namespace Filtration.ItemFilterPreview.Data.Repositories
{
    public class ItemSetRepository : IEntityRepository<ItemSet>
    {
        FiltrationDbContext _context = new FiltrationDbContext();


        public IQueryable<ItemSet> All => _context.ItemSets;

        public IQueryable<ItemSet> AllIncluding(params Expression<Func<ItemSet, object>>[] includeProperties)
        {
            IQueryable<ItemSet> query = _context.ItemSets;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return query;
        }

        public ItemSet Find(int id)
        {
            return _context.ItemSets.Find(id);
        }

        public void InsertOrUpdate(ItemSet itemSet)
        {
            if (itemSet.Id == default(long))
            {
                // New entity
                _context.ItemSets.Add(itemSet);
            }
            else
            {
                // Existing entity
                _context.Entry(itemSet).State = EntityState.Modified;
            }
        }

        public void Delete(int id)
        {
            var itemSet = _context.ItemSets.Find(id);
            _context.ItemSets.Remove(itemSet);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
