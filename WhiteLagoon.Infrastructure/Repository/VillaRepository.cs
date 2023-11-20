using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class VillaRepository : IVillaRepository
    {
        private readonly ApplicationDbContext _db;

        public VillaRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        
        public Villa Get(Expression<Func<Villa, bool>> filter, string? includeProperties)
        {
            IQueryable<Villa> query = _db.Set<Villa>();

            if (filter != null)
            {
                query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                //Villa, VillaNumber -- case sensitive
                foreach (var includeProp in includeProperties.Split(new char[','], StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        IEnumerable<Villa> IVillaRepository.GetAll(Expression<Func<Villa, bool>>? filter, string? includeProperties)
        {
            IQueryable<Villa> query = _db.Set<Villa>();
            
            if (filter != null)
            {
                query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                //Villa, VillaNumber -- case sensitive
                foreach (var includeProp in includeProperties.Split(new char[','], StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }
        void IVillaRepository.Add(Villa entity)
        {
            _db.Add(entity);
        }
        void IVillaRepository.Update(Villa entity)
        {
            _db.Villas.Update(entity);
        }

        void IVillaRepository.Remove(Villa entity)
        {
            _db.Remove(entity);
        }

        void IVillaRepository.Save()
        {
            _db.SaveChanges();
        }        
    }
}
