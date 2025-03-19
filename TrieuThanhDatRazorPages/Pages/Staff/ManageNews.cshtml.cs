using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.API.Hubs;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace TrieuThanhDatRazorPages.Pages.Staff
{

    [Authorize(Roles = "Staff")]
    public class ManageNewsModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly ITagService _tagService;
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<NewsHub> _newsHub;

        public ManageNewsModel
        (
            INewsService newsService,
            ITagService tagService,
            ICategoryService categoryService,
            IHubContext<NewsHub> newsHub
        )
        {
            _newsService = newsService;
            _tagService = tagService;
            _categoryService = categoryService;
            _newsHub = newsHub;
        }

        [BindProperty]
        public NewsArticleDTO NewsArticle { get; set; }
        public List<NewsArticleDTO> NewsList { get; set; } = new();
        public List<CategoryDTO> Categories { get; set; } = new();
        public string userID;

        public async Task<IActionResult> OnGetAsync()
        {
            userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userID) || !short.TryParse(userID, out short userId))
            {
                return Unauthorized();
            }

            // Load news articles for this staff
            NewsList = (await _newsService.GetAllNewsAsync())
                .Select(n => new NewsArticleDTO
                {
                    NewsArticleID = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    CreatedDate = n.CreatedDate.GetValueOrDefault(),
                    NewsContent = n.NewsContent ?? string.Empty,
                    NewsSource = n.NewsSource ?? "Unknown",
                    CategoryID = n.CategoryId.GetValueOrDefault(-1),
                    CategoryName = n.Category != null ? n.Category.CategoryName : "Uncategorized",
                    NewsStatus = n.NewsStatus ?? false,
                    CreatedByID = n.CreatedById.GetValueOrDefault(-1),
                    UpdatedByID = n.UpdatedById.GetValueOrDefault(-1),
                    ModifiedDate = n.ModifiedDate,
                    // Convert Tags properly
                    Tags = n.Tags.Select(t => new Tag
                    {
                        TagId = t.TagId,
                        TagName = t.TagName
                    }).ToList()
                })
                .ToList();

            // Convert List<Category> to List<CategoryDTO>
            Categories = (await _categoryService.GetAllCategoriesAsync())
                .Select(c => new CategoryDTO
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDesciption,
                    ParentCategoryID = c.ParentCategoryId.GetValueOrDefault(),
                })
                .ToList();

            return Page();
        }
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostCreateOrUpdateAsync([FromBody] NewsArticleDTO newsArticleDto)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid data" });
            }

            try
            {
                // Handle Tags
                if (!string.IsNullOrEmpty(newsArticleDto.TagNames))
                {
                    var tagNames = newsArticleDto.TagNames.Split(',')
                        .Select(tag => tag.Trim())
                        .Distinct()
                        .ToList();

                    List<Tag> tagList = new();

                    foreach (var tagName in tagNames)
                    {
                        var existingTag = await _tagService.GetTagByNameAsync(tagName);
                        if (existingTag != null)
                        {
                            tagList.Add(existingTag); // Use existing tag
                        }
                        else
                        {
                            // Create a new tag
                            var newTag = new Tag { TagName = tagName };
                            await _tagService.AddTagAsync(newTag);
                            tagList.Add(newTag);
                        }
                    }

                    newsArticleDto.Tags = tagList;
                }
                if (string.IsNullOrEmpty(newsArticleDto.NewsArticleID))
                {
                    // Create New Article
                    await _newsService.AddNewsAsync(newsArticleDto);
                }
                else
                {
                    // Update Existing Article
                    await _newsService.UpdateNewsAsync(newsArticleDto);
                }

                return new JsonResult(new { success = true, message = "News saved successfully!" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnPostCreateNewsAsync([FromBody] NewsArticleDTO newArticle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !short.TryParse(userIdClaim, out short userId))
            {
                return Unauthorized();
            }

            newArticle.CreatedByID = userId;
            var createdNews = await _newsService.AddNewsAsync(newArticle);

            if (createdNews != null)
            {
                await _newsHub.Clients.All.SendAsync("ReceiveNewsUpdate", createdNews);
                return new JsonResult(new { success = true, data = createdNews });
            }

            return BadRequest("Failed to create news article.");
        }

        public async Task<IActionResult> OnPostUpdateNewsAsync([FromBody] NewsArticleDTO updatedArticle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingNews = await _newsService.GetNewsByIdAsync(updatedArticle.NewsArticleID);
            if (existingNews == null)
            {
                return NotFound("News article not found.");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingNews.CreatedByID.GetValueOrDefault(-1).ToString() != userIdClaim)
            {
                return Unauthorized();
            }

            var updated = await _newsService.UpdateNewsAsync(updatedArticle);
            if (updated)
            {
                await _newsHub.Clients.All.SendAsync("ReceiveNewsUpdate", updatedArticle);
                return new JsonResult(new { success = true, data = updatedArticle });
            }

            return BadRequest("Failed to update news article.");
        }

        public async Task<IActionResult> OnPostDeleteNewsAsync([FromBody] int newsId)
        {
            var existingNews = await _newsService.GetNewsByIdAsync(newsId);
            if (existingNews == null)
            {
                return NotFound("News article not found.");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingNews.CreatedByID.GetValueOrDefault(-1).ToString() != userIdClaim)
            {
                return Unauthorized();
            }

            var deleted = await _newsService.DeleteNewsAsync(newsId);
            if (deleted)
            {
                await _newsHub.Clients.All.SendAsync("ReceiveNewsDelete", newsId);
                return new JsonResult(new { success = true });
            }

            return BadRequest("Failed to delete news article.");
        }
    }

}
