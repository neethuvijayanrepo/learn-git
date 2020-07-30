using TestProject.Data.DataInterfaces.Common;
using TestProject.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Data.Repositories.Common
{
    /// <summary>
    /// Base repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity for which instance is to be created.</typeparam>
    internal abstract class TestProjectRepository<TEntity> where TEntity : class
    {
        private readonly ITestProjectContext _dbContext;
        private readonly DbSet<TEntity> _dbEntity;

        /// <summary>
        /// Base repository constructor
        /// </summary>
        /// <param name="context">Instance of DB Context <see cref="ITestProjectContext"/></param>
        protected internal TestProjectRepository(ITestProjectContext context)
        {
            this._dbContext = context;

            this._dbEntity = _dbContext.Set<TEntity>();
        }

        /// <summary>
        /// To add a new Entity in DB.
        /// </summary>
        /// <param name="entity">Object of the entity which is to be added.</param>
        /// <returns>Copy of object added in DB.</returns>
        public virtual TEntity Create(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Added;
            _dbContext.SaveChanges();
            return entity;
        }

        /// <summary>
        /// To update an entity in DB.
        /// </summary>
        /// <param name="entity">Object of the entity which is to be updated</param>
        /// <returns>Copy of object updated in DB.</returns>
        public virtual TEntity Update(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            _dbContext.SaveChanges();
            return entity;
        }

        /// <summary>
        /// To get an entity by its Id.
        /// </summary>
        /// <param name="id">Id of the entity which is to be fetched.</param>
        /// <returns>Entity object or null if its not exists.</returns>
        public virtual TEntity GetById(object id)
        {
            return _dbEntity.Find(id);
        }

        /// <summary>
        /// To get all entities of a DBSet.
        /// </summary>
        /// <returns>All entities in DBSet (Table).</returns>
        public virtual IEnumerable<TEntity> GetAll()
        {
            return _dbEntity.ToList();
        }

        /// <summary>
        /// To filter and get some entities from DB, including some referring entities and sorting if specified.
        /// </summary>
        /// <param name="filter">The filter condition.</param>
        /// <param name="includeRefs">Specify the referring entities which are to be included if required and null otherwise</param>
        /// <param name="orderBy">Specify the order by condition if required and null otherwise.</param>
        /// <param name="sortOrder">Specify the sort order. Default will be ASC.</param>
        /// <returns>Collection of entities which are satisfying the requirements.</returns>
        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, string[] includeRefs = null,
            Func<TEntity, object> orderBy = null, TestProjectSortOrder sortOrder = TestProjectSortOrder.ASC)
        {
            var filteredList = _dbEntity.Where(filter);

            if (includeRefs != null && includeRefs.Length > 0)
            {
                foreach (string refProp in includeRefs)
                {
                    filteredList = filteredList.Include(refProp);
                }
            }

            if (orderBy == null)
            {
                return filteredList.AsEnumerable().ToList();
            }

            if (sortOrder == TestProjectSortOrder.ASC)
            {
                return filteredList.AsEnumerable().OrderBy(orderBy).ToList();
            }
            else
            {
                return filteredList.AsEnumerable().OrderByDescending(orderBy).ToList();
            }
        }
    }
}
