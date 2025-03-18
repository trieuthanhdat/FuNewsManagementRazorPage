using FUNewsManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<NewsArticle>> GetAllNewsAsync();
        Task AddNewsAsync(NewsArticle newsArticle);
    }
}
