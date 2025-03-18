using FUNewsManagement.Domain;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Infrastructure.Interfaces;
using FUNewsManagement.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.Infrastructure.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FunewsManagementContext _context;

        public IGenericRepository<NewsArticle> NewsArticles { get; }
        public IGenericRepository<Category> Categories { get; }
        public IGenericRepository<SystemAccount> Accounts { get; }

        public UnitOfWork(FunewsManagementContext context)
        {
            _context = context;
            NewsArticles = new GenericRepository<NewsArticle>(context);
            Categories = new GenericRepository<Category>(context);
            Accounts = new GenericRepository<SystemAccount>(context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }

}
