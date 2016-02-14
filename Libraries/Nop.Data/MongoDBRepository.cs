using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Nop.Data
{
    /// <summary>
    /// MongoDB repository
    /// </summary>
    public partial class MongoDBRepository<T> : IRepository<T> where T : BaseEntity
    {
        #region Fields
        private static readonly Object _locker = new object();
        /// <summary>
        /// Mongo Database
        /// </summary>
        private IMongoDatabase database;

        /// <summary>
        /// Gets the the collection
        /// </summary>
        private IMongoCollection<T> collection;

        #endregion

        #region Ctor

        /// <summary>
        /// Gets the the collection
        /// </summary>
        public IMongoCollection<T>  Collection
        {
            get
            {
                return collection;
            }
        }
        public IMongoDatabase Database
        {
            get
            {
                return database;
            }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public MongoDBRepository()
        {
            string connectionString = DataSettingsHelper.ConnectionString();
            var client = new MongoClient(connectionString);
            var databaseName = new MongoUrl(connectionString).DatabaseName;
            database = client.GetDatabase(databaseName);
            collection = database.GetCollection<T>(typeof(T).Name);
        }

        public MongoDBRepository(IMongoClient client)
        {
            string connectionString = DataSettingsHelper.ConnectionString();
            var databaseName = new MongoUrl(connectionString).DatabaseName;
            database = client.GetDatabase(databaseName);
            collection = database.GetCollection<T>(typeof(T).Name);
        }


        #endregion

        #region Methods

        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        public virtual T GetById(int id)
        {
            return this.collection.Find(e => e.Id == id).FirstOrDefaultAsync().Result;
        }

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual T Insert(T entity)
        {
            lock (_locker)
            {
                var newId = ObjectId.GenerateNewId();
                entity._id = newId.ToString();

                if (entity.Id == 0)
                {
                    var resultMax = this.collection.Find(e => true).SortByDescending(x => x.Id).FirstOrDefaultAsync().Result;
                    entity.Id = resultMax != null ? resultMax.Id + 1 : 1;                    
                }
                else
                {
                    var result = this.collection.Find(e => e.Id == entity.Id).ToListAsync().Result;
                    if (result.Count > 0)
                    {
                        var resultMax = this.collection.Find(e => true).SortByDescending(x => x.Id).FirstOrDefaultAsync().Result;
                        entity.Id = resultMax != null ? resultMax.Id + 1 : 1;
                    }
                }
                this.collection.InsertOne(entity);
                return entity;
            }
        }

        /// <summary>
        /// Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Insert(IEnumerable<T> entities)
        {
            //await this.collection.InsertManyAsync(entities);
            foreach (var entity in entities)
                Insert(entity);
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual T Update(T entity)
        {
            var update = this.collection.ReplaceOneAsync(x=>x._id == entity._id, entity, new UpdateOptions() { IsUpsert = false }).Result;
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
            this.collection.FindOneAndDeleteAsync(e => e.Id == entity.Id);           
        }

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Delete(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                this.collection.FindOneAndDeleteAsync(e => e.Id == entity.Id);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IMongoQueryable<T> Table
        {
            get { return this.collection.AsQueryable(); }
        }
        #endregion

    }
}