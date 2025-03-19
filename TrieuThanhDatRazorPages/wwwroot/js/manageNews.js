$(document).ready(function () {

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/newsHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.start()
        .then(() => console.log("✅ Connected to SignalR!"))
        .catch(err => console.error("❌ SignalR Connection Error:", err.toString()));

    // Search Functionality
    $("#searchBox").on("input", function () {
        let searchText = $(this).val().toLowerCase();
        $("#newsTable tbody tr").each(function () {
            let title = $(this).find("td:eq(1)").text().toLowerCase();
            let headline = $(this).find("td:eq(2)").text().toLowerCase();
            $(this).toggle(title.includes(searchText) || headline.includes(searchText));
        });
    });

    $("#newsForm").submit(function (event) {
        event.preventDefault();

        let formData = new FormData();
        formData.append("NewsArticleID", $("#newsId").val() || "0");
        formData.append("NewsTitle", $("#newsTitle").val().trim());
        formData.append("Headline", $("#newsHeadline").val().trim());
        formData.append("NewsContent", $("#newsContent").val().trim());
        formData.append("NewsSource", $("#newsSource").val().trim() || "");
        formData.append("CategoryID", $("#newsCategory").val());
        formData.append("NewsStatus", $("#newsStatus").val());
        formData.append("TagNames", $("#newsTags").val().trim());

        console.log("Sending Data: ", JSON.stringify(Object.fromEntries(formData)));

        $.ajax({
            url: "?handler=CreateOrUpdate",
            type: "POST",
            processData: false,
            contentType: false,
            data: formData,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    $("#newsModal").modal("hide");
                    loadNewsArticles();
                    alert(response.message);
                } else {
                    alert("❌ Error: " + response.message);
                    console.log("Error Data: ", JSON.stringify(response.data));
                }
            },
            error: function (xhr) {
                console.error("🚨 Server Error:", xhr.status, xhr.responseText);
                alert("⚠️ Failed to create/update news.");
            }
        });
    });
    loadNewsArticles();
});

// Load News Articles
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

// Render a Single News Row
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

// Open Create News Modal
function openCreateModal() {
    $("#modalTitle").html('<i class="fas fa-plus"></i> Add News');
    $("#newsId").val("");
    $("#newsTitle, #newsHeadline, #newsContent, #newsTags, #newsSource").val("");
    $("#newsCategory").val("");
    $("#newsStatus").val("true");
    $("#newsModal").modal("show");
}

// Open Edit News Modal
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
                $("#newsTags").val(news.tagNames || "");  // ✅ Populate tags correctly
                $("#newsSource").val(news.newsSource || "");
                $("#newsCategory").val(news.categoryID);
                $("#newsStatus").val(news.newsStatus.toString());
                $("#newsModal").modal("show");
            } else {
                alert("❌ Error fetching news details.");
            }
        },
        error: function (xhr) {
            console.error("🚨 Error fetching news details:", xhr.responseText);
        }
    });
}
function confirmDelete(newsId) {
    if (!confirm("Are you sure you want to delete this article?")) return;

    console.log(`🔍 Attempting to delete NewsArticleID: ${newsId}`);

    $.ajax({
        url: "?handler=DeleteNews",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ newsId: newsId }), // Ensure data is correctly formatted
        success: function (response) {
            console.log(" Response from server:", response);
            if (response.success) {
                console.log(`News article ${newsId} deleted successfully.`);
                $(`#newsRow-${newsId}`).fadeOut(500, function () {
                    $(this).remove();
                });
                alert("News deleted successfully!");
            } else {
                console.warn("⚠️ Server returned an error:", response.message);
                alert(response.message);
            }
        },
        error: function (xhr) {
            console.error("❌ Error deleting news:", xhr.status, xhr.statusText);
            console.error("📜 Response Text:", xhr.responseText);
            alert("Failed to delete news article. Check console for details.");
        }
    });
}

