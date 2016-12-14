using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Grand.Data
{
    /// <summary>
    /// MongoDB repository
    /// </summary>
    public partial class MongoDBRepository<T> : IRepository<T> where T : BaseEntity
    {
        #region Fields

        /// <summary>
        /// Gets the collection
        /// </summary>
        protected IMongoCollection<T> _collection;       
        public IMongoCollection<T>  Collection
        {
            get
            {
                return _collection;
            }
        }

        /// <summary>
        /// Mongo Database
        /// </summary>
        protected IMongoDatabase _database;
        public IMongoDatabase Database
        {
            get
            {
                return _database;
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public MongoDBRepository()
        {

        }
        public MongoDBRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var databaseName = new MongoUrl(connectionString).DatabaseName;
            _database = client.GetDatabase(databaseName);
            _collection = _database.GetCollection<T>(typeof(T).Name);
        }

        public MongoDBRepository(IMongoClient client)
        {
            string connectionString = DataSettingsHelper.ConnectionString();
            var databaseName = new MongoUrl(connectionString).DatabaseName;            
            _database = client.GetDatabase(databaseName);
            _collection = _database.GetCollection<T>(typeof(T).Name);
        }

        public MongoDBRepository(IMongoClient client, IMongoDatabase mongodatabase)
        {
            _database = mongodatabase;
            _collection = _database.GetCollection<T>(typeof(T).Name);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        public virtual T GetById(string id)
        {
            return this._collection.Find(e => e.Id == id).FirstOrDefaultAsync().Result;
        }

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual T Insert(T entity)
        {
            this._collection.InsertOne(entity);
            return entity;
        }

        /// <summary>
        /// Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Insert(IEnumerable<T> entities)
        {            
            foreach (var entity in entities)
                Insert(entity);
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual T Update(T entity)
        {
            var update = this._collection.ReplaceOneAsync(x=>x.Id == entity.Id, entity, new UpdateOptions() { IsUpsert = false }).Result;
            return entity;

        }

        /// <summary>
        /// Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Update(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                Update(entity);
            }
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Delete(T entity)
        {
            this._collection.FindOneAndDeleteAsync(e => e.Id == entity.Id);           
        }

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Delete(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                this._collection.FindOneAndDeleteAsync(e => e.Id == entity.Id);
            }
        }


        #endregion

        #region Properties

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IMongoQueryable<T> Table
        {
            get { return this._collection.AsQueryable(); }
        }

        #endregion

    }
}