using FUNewsManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<NewsArticle> NewsArticles { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<SystemAccount> Accounts { get; }

        Task<int> SaveChangesAsync();
    }

}
