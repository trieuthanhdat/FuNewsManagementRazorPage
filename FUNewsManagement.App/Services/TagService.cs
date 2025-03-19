using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.App.Services
{
    public class TagService : ITagService
    {
        private readonly IGenericRepository<Tag> _tagRepository;

        public TagService(IGenericRepository<Tag> tagRepository)
        {
            _tagRepository = tagRepository;
        }

        // Get all tags
        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        // Get a tag by its ID
        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            return await _tagRepository.GetByIdAsync(tagId);
        }

        // Get a tag by its name
        public async Task<Tag?> GetTagByNameAsync(string tagName)
        {
            return await _tagRepository.GetFirstOrDefaultAsync(filter: t => t.TagName == tagName);
        }

        // Add a new tag
        public async Task AddTagAsync(Tag tag)
        {
            await _tagRepository.AddAsync(tag);
        }

        // Update an existing tag
        public async Task UpdateTagAsync(Tag tag)
        {
            await _tagRepository.UpdateAsync(tag);
        }

        // Delete a tag by ID
        public async Task DeleteTagAsync(int tagId)
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag != null)
            {
                await _tagRepository.DeleteAsync(tagId);
            }
        }
    }

}
