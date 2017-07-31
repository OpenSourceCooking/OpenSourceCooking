using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSourceCooking.Models
{
    public class GroupStoreBase
    {
        public DbContext Context
        {
            get;
            private set;
        }
        public DbSet<ApplicationGroup> DbEntitySet
        {
            get;
            private set;
        }
        public IQueryable<ApplicationGroup> EntitySet
        {
            get
            {
                return DbEntitySet;
            }
        }
        public GroupStoreBase(DbContext context)
        {
            Context = context ?? throw new ArgumentNullException("context");
            DbEntitySet = context.Set<ApplicationGroup>();
        }
        public void Create(ApplicationGroup entity)
        {
            DbEntitySet.Add(entity);
        }
        public void Delete(ApplicationGroup entity)
        {
            DbEntitySet.Remove(entity);
        }
        public virtual Task<ApplicationGroup> GetByIdAsync(object id)
        {
            return DbEntitySet.FindAsync(new object[] { id });
        }
        public virtual ApplicationGroup GetById(object id)
        {
            return DbEntitySet.Find(new object[] { id });
        }
        public virtual void Update(ApplicationGroup entity)
        {
            if (entity != null)            
                Context.Entry(entity).State = EntityState.Modified;            
        }
    }
}