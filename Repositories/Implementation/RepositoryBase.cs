﻿using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repositories.Implementation
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        internal OrcusUMSContext db;
        internal DbSet<T> _dbSet;

        //private readonly IDbContextTransaction transaction;

        internal RepositoryBase()
        {
            db = new OrcusUMSContext(new DbContextOptions<OrcusUMSContext>());
            _dbSet = db.Set<T>();
            //transaction = db.Database.BeginTransaction();
        }

        internal RepositoryBase(OrcusUMSContext context)
        {
            db = context;
            _dbSet = db.Set<T>();
        }
        
        public virtual void Add(T entity)
        {
            _dbSet.Add(entity);
            db.SaveChanges();
        }
        public virtual T Get(Expression<Func<T, bool>> where)
        {
            DetachAllEntities();
            return _dbSet.Where(where).FirstOrDefault();
        }
        public virtual T Get(int id)
        {
            DetachAllEntities();
            return _dbSet.Find(id);
        }
        public virtual T Get(decimal id)
        {
            DetachAllEntities();
            return _dbSet.Find(id);
        }
        public virtual T Get(float id)
        {
            DetachAllEntities();
            return _dbSet.Find(id);
        }
        public virtual T Get(string id)
        {
            DetachAllEntities();
            return _dbSet.Find(id);
        }
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }
        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity); 
            db.SaveChanges();
        }
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            var objects = _dbSet.Where(where).AsEnumerable();
            foreach (var obj in objects)
            {
                _dbSet.Remove(obj);
            }
        }
        public virtual IQueryable<T> AsQueryable() => _dbSet.AsNoTracking().AsQueryable();
        public virtual IEnumerable<T> GetAll() => _dbSet.ToList();
        public virtual void Commit()
        {
            DetachAllEntities();
        }
        public virtual void Save() => db.SaveChanges();
        public virtual void Rollback()
        {
            DetachAllEntities();
        }
        public virtual void Dispose()
        {
            db.Dispose();
        }
        public void DetachAllEntities()
        {
            IEnumerable<EntityEntry> changedEntriesCopy = db.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified
                        || x.State == EntityState.Added
                        || x.State == EntityState.Deleted);
            foreach (var entity in changedEntriesCopy)
            {
                entity.State = EntityState.Detached;
            }
        }
        public int GetMaxPK(string pkPropertyName)
        {
            // TODO: add argument checks
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.Property(parameter, pkPropertyName);
            var lambda = Expression.Lambda<Func<T, int>>(body, parameter);
            var result = _dbSet.Max(lambda);
            return result;
        }
    }
}
