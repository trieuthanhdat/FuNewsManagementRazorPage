$(document).ready(function () {
    loadNewsArticles();

    // Search Functionality
    $("#searchBox").on("input", function () {
        let searchText = $(this).val().toLowerCase();
        $("#newsTable tbody tr").each(function () {
            let title = $(this).find("td:eq(1)").text().toLowerCase();
            let headline = $(this).find("td:eq(2)").text().toLowerCase();
            $(this).toggle(title.includes(searchText) || headline.includes(searchText));
        });
    });

    // Prevent duplicate event binding
    $("#newsForm").off("submit").on("submit", function (event) {
        event.preventDefault();

        let formData = {
            NewsArticleID: $("#newsId").val() ? parseInt($("#newsId").val()) : 0,
            NewsTitle: $("#newsTitle").val().trim(),
            Headline: $("#newsHeadline").val().trim(),
            NewsContent: $("#newsContent").val().trim(),
            NewsSource: $("#newsSource").val().trim(),
            CategoryID: $("#newsCategory").val(),
            NewsStatus: $("#newsStatus").val() === "true", // Convert to boolean
            TagNames: $("#newsTags").val().trim() // Tags as a single string
        };

        console.log("Creating/Updating News: ", JSON.stringify(formData));

        $.ajax({
            url: "?handler=CreateOrUpdate",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    $("#newsModal").modal("hide");
                    loadNewsArticles(); // ✅ Refresh only the data, not the whole page
                    alert(response.message);
                } else {
                    alert(response.message);
                }
            },
            error: function (xhr) {
                console.error("Error:", xhr.responseText);
            }
        });
    });
});

// ✅ Load News Articles
function loadNewsArticles() {
    $.ajax({
        url: "?handler=GetNews",
        type: "GET",
        dataType: "json",
        success: function (response) {
            let tableBody = $("#newsTable tbody");
            tableBody.empty();

            if (response.success && response.data.length > 0) {
                response.data.forEach(article => {
                    tableBody.append(renderNewsRow(article));
                });
            } else {
                tableBody.html(`<tr><td colspan="10" class="text-center text-warning">No news articles found.</td></tr>`);
            }
        },
        error: function (xhr) {
            console.error("Error loading news:", xhr.responseText);
        }
    });
}

// ✅ Render a Single News Row
function renderNewsRow(article) {
    return `
        <tr id="newsRow-${article.newsArticleID}">
            <td>${article.newsArticleID}</td>
            <td>${article.newsTitle}</td>
            <td>${article.headline}</td>
            <td>${article.createdDate}</td>
            <td>${article.categoryName}</td>
            <td>${article.newsSource || "N/A"}</td>
            <td>${article.newsStatus ? "Active" : "Inactive"}</td>
            <td>${article.modifiedDate || "N/A"}</td>
            <td>${article.createdByID}</td>
            <td>${article.tagNames || "No Tags"}</td>
            <td>
                <button class="btn btn-warning btn-sm" onclick="openEditModal(${article.newsArticleID})">
                    <i class="fas fa-edit"></i> Edit
                </button>
                <button class="btn btn-danger btn-sm" onclick="confirmDelete(${article.newsArticleID})">
                    <i class="fas fa-trash"></i> Delete
                </button>
            </td>
        </tr>
    `;
}

// ✅ Open Create News Modal
function openCreateModal() {
    $("#modalTitle").html('<i class="fas fa-plus"></i> Add News');
    $("#newsId").val("");
    $("#newsTitle, #newsHeadline, #newsContent, #newsTags, #newsSource").val("");
    $("#newsCategory").val("");
    $("#newsStatus").val("true");
    $("#newsModal").modal("show");
}

// ✅ Open Edit News Modal
function openEditModal(newsId) {
    $.ajax({
        url: "?handler=GetNewsById",
        type: "GET",
        data: { newsId: newsId },
        dataType: "json",
        success: function (response) {
            if (response.success) {
                let news = response.data;
                $("#modalTitle").html('<i class="fas fa-edit"></i> Edit News');
                $("#newsId").val(news.newsArticleID);
                $("#newsTitle").val(news.newsTitle);
                $("#newsHeadline").val(news.headline);
                $("#newsContent").val(news.newsContent);
                $("#newsTags").val(news.tagNames || ""); // ✅ Fixed tags display
                $("#newsSource").val(news.newsSource);
                $("#newsCategory").val(news.categoryID);
                $("#newsStatus").val(news.newsStatus.toString());
                $("#newsModal").modal("show");
            } else {
                alert("Error fetching news details.");
            }
        },
        error: function (xhr) {
            console.error("Error fetching news details:", xhr.responseText);
        }
    });
}

// Confirm & Delete News
function confirmDelete(newsId) {
    if (!confirm("Are you sure you want to delete this article?")) return;

    $.ajax({
        url: "?handler=DeleteNews",
        type: "POST",
        data: JSON.stringify({ newsId: newsId }),
        contentType: "application/json",
        success: function (response) {
            if (response.success) {
                $(`#newsRow-${newsId}`).fadeOut(500, function () {
                    $(this).remove();
                });
                alert("News deleted successfully!");
            } else {
                alert("Error deleting news.");
            }
        },
        error: function (xhr) {
            console.error("Error deleting news:", xhr.responseText);
        }
    });
}
