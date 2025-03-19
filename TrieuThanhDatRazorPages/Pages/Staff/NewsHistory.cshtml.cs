using FUNewsManagement.API.Hubs;
using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.DTOs;
using FUNewsManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TrieuThanhDatRazorPages.Pages.Staff
{
    public class NewsHistoryModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly ITagService _tagService;
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<NewsHub> _newsHub;

        public NewsHistoryModel
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
            bool isSuccess = await OnGetData();
            if (!isSuccess) return Unauthorized();

            return Page();
        }

        private async Task<bool> OnGetData()
        {
            userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userID) || !short.TryParse(userID, out short userId))
            {
                return false;
            }

            // Load news articles for this staff
            NewsList = (await _newsService.GetNewsByAuthorAsync(userId))
                .Select(n => new NewsArticleDTO
                {
                    NewsArticleID = n.NewsArticleID,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    CreatedDate = n.CreatedDate,
                    NewsContent = n.NewsContent ?? string.Empty,
                    NewsSource = n.NewsSource ?? "Unknown",
                    CategoryID = n.CategoryID.GetValueOrDefault(-1),
                    CategoryName = n.CategoryName,
                    NewsStatus = n.NewsStatus,
                    CreatedByID = n.CreatedByID.GetValueOrDefault(-1),
                    UpdatedByID = n.UpdatedByID.GetValueOrDefault(-1),
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
            return true;
        }

        public async Task<IActionResult> OnGetGetNewsAsync()
        {
            bool isSuccess = await OnGetData();
            return new JsonResult(new { success = isSuccess, message = $"News articles Get {isSuccess}!", data = NewsList });
        }
        public async Task<IActionResult> OnPostCreateOrUpdateAsync()
        {
            try
            {
                var form = Request.Form;
                var isNew = string.IsNullOrEmpty(form["NewsArticleID"]) || form["NewsArticleID"] == "-1";

                var newsArticleDto = new NewsArticleDTO
                {
                    NewsArticleID = isNew ? Guid.NewGuid().ToString() : form["NewsArticleID"],
                    NewsTitle = form["NewsTitle"],
                    Headline = form["Headline"],
                    NewsContent = form["NewsContent"],
                    NewsSource = string.IsNullOrEmpty(form["NewsSource"]) ? string.Empty : form["NewsSource"],
                    CategoryID = short.TryParse(form["CategoryID"], out short categoryId) ? categoryId : null,
                    NewsStatus = form["NewsStatus"] == "true",
                    TagNames = form["TagNames"]
                };

                // Validation Check
                if (string.IsNullOrEmpty(newsArticleDto.NewsTitle) || string.IsNullOrEmpty(newsArticleDto.NewsContent))
                {
                    return BadRequest(new { success = false, message = "Title and Content are required." });
                }

                // If creating new NewsArticle
                if (isNew)
                {
                    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!short.TryParse(userIdClaim, out short userId))
                    {
                        return Unauthorized();
                    }

                    newsArticleDto.CreatedDate = DateTime.UtcNow;
                    newsArticleDto.CreatedByID = userId;

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
                                tagList.Add(existingTag);
                            }
                            else
                            {
                                var newTag = new Tag { TagName = tagName };
                                await _tagService.AddTagAsync(newTag);
                                tagList.Add(newTag);
                            }
                        }
                        newsArticleDto.Tags = tagList;
                    }

                    // Create new NewsArticle
                    bool isSuccess = await _newsService.AddNewsAsync(newsArticleDto);
                    return new JsonResult(new { success = isSuccess, message = $"News article created successfully! ID: {newsArticleDto.NewsArticleID}" });
                }
                else
                {
                    // Update existing NewsArticle
                    newsArticleDto.ModifiedDate = DateTime.UtcNow;
                    bool isSuccess = await _newsService.UpdateNewsAsync(newsArticleDto);
                    return new JsonResult(new { success = isSuccess, message = $"News article updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteNewsAsync([FromBody] string newsId)
        {
            try
            {
                if (string.IsNullOrEmpty(newsId))
                {
                    return new JsonResult(new { success = false, message = "News Article Not Found!" });
                }
                var existingNews = await _newsService.GetNewsByIdAsync(newsId);
                if (existingNews == null)
                {
                    return new JsonResult(new { success = false, message = "News Article Not Found!" });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (existingNews.CreatedByID.GetValueOrDefault(-1).ToString() != userIdClaim)
                {
                    return new JsonResult(new { success = false, message = "You are not athorized to delete this!" }); ;
                }

                var deleted = await _newsService.DeleteNewsAsync(newsId);
                if (deleted)
                {
                    await _newsHub.Clients.All.SendAsync("ReceiveNewsDelete", newsId);
                    return new JsonResult(new { success = true, message = "Successfully deleted the article!" });
                }

                return BadRequest("Failed to delete news article.");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"❌ Server error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }
    }
}
