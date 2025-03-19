$(document).ready(function () {
    loadCategories(); // Load categories on page load

    // Search Functionality
    $("#searchCategoryBox").on("input", function () {
        let searchText = $(this).val().toLowerCase();
        $("#categoriesTable tbody tr").each(function () {
            let categoryName = $(this).find("td:eq(1)").text().toLowerCase();
            $(this).toggle(categoryName.includes(searchText));
        });
    });

    // Form Submission for Create / Edit
    $("#categoryForm").submit(function (event) {
        event.preventDefault();
        let categoryId = $("#categoryId").val();
        let categoryName = $("#categoryName").val().trim();
        let categoryDescription = $("#categoryDescription").val().trim();
        let parentCategoryId = $("#parentCategory").val() || null;
        let isActive = $("#categoryStatus").val() === "true";

        if (!categoryName) {
            showAlert("Category name is required!", "danger");
            return;
        }

        let requestData = {
            CategoryID: categoryId ? parseInt(categoryId) : 0,
            CategoryName: categoryName,
            CategoryDescription: categoryDescription,
            ParentCategoryID: parentCategoryId ? parseInt(parentCategoryId) : null,
            IsActive: isActive
        };

        $.ajax({
            url: "?handler=CreateOrUpdateCategory",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
                    $("#categoryModal").modal("hide");
                    loadCategories();
                    showAlert(response.message, "success");
                } else {
                    showAlert(response.message, "danger");
                }
            },
            error: function (xhr) {
                console.error("Error:", xhr.responseText);
                showAlert("Failed to save category!", "danger");
            }
        });
    });
});

// Fetch Categories from API
function loadCategories() {
    $.ajax({
        url: "?handler=GetCategories",
        type: "GET",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                let tableBody = $("#categoriesTable tbody");
                tableBody.empty();
                response.data.forEach(category => {
                    tableBody.append(renderCategoryRow(category));
                });
            } else {
                $("#categoriesTable tbody").html(`<tr><td colspan="5" class="text-center text-warning">No categories found.</td></tr>`);
            }
        },
        error: function (xhr) {
            console.error("Error loading categories:", xhr.responseText);
        }
    });
}

// Render a Single Category Row
function renderCategoryRow(category) {
    return `
        <tr id="categoryRow-${category.categoryID}">
            <td>${category.categoryID}</td>
            <td>${category.categoryName}</td>
            <td>${category.categoryDescription || "N/A"}</td>
            <td>${category.isActive ? "Active" : "Inactive"}</td>
            <td>
                <button class="btn btn-warning btn-sm" onclick="openEditModal(${category.categoryID})">
                    <i class="fas fa-edit"></i> Edit
                </button>
                <button class="btn btn-danger btn-sm" onclick="confirmDelete(${category.categoryID})">
                    <i class="fas fa-trash"></i> Delete
                </button>
            </td>
        </tr>
    `;
}

// Open Create Category Modal
function openCreateModal() {
    $("#modalTitle").html('<i class="fas fa-plus"></i> Add Category');
    $("#categoryId").val("");
    $("#categoryName, #categoryDescription").val("");
    $("#categoryStatus").val("true");
    $("#categoryModal").modal("show");
}

// Open Edit Category Modal
function openEditModal(categoryId) {
    const token = $('input[name="__RequestVerificationToken"]').val();
    let urlApi = "?handler=GetCategoryById";
    let httpMethod = "GET";

    $.ajax({
        url: urlApi,
        data: { id: categoryId },
        type: httpMethod,
        dataType: "json",
        headers: {
            "RequestVerificationToken": token
        },
        success: function (response) {
            if (response.success) {
                let category = response.data;
                $("#modalTitle").html('<i class="fas fa-edit"></i> Edit Category');
                $("#categoryId").val(category.categoryID);
                $("#categoryName").val(category.categoryName);
                $("#categoryDescription").val(category.categoryDescription);
                $("#categoryStatus").val(category.isActive ? "true" : "false");
                $("#categoryModal").modal("show");
            } else {
                showAlert("Error fetching category details.", "danger");
            }
        },
        error: function (xhr) {
            console.error("Error fetching category details:", xhr.responseText);
        }
    });
}

// Confirm & Delete Category
function confirmDelete(categoryId) {
    if (!confirm("Are you sure you want to delete this category?")) return;

    $.ajax({
        url: "?handler=DeleteCategory",
        type: "POST",
        contentType: "application/json",
        data: { categoryId: categoryId },
        success: function (response) {
            if (response.success) {
                $(`#categoryRow-${categoryId}`).fadeOut(500, function () {
                    $(this).remove();
                });
                showAlert("Category deleted successfully!", "success");
            } else {
                showAlert("Cannot delete this category. It may be linked to News Articles.", "danger");
            }
        },
        error: function (xhr) {
            console.error("Error deleting category:", xhr.responseText);
            showAlert("Failed to delete category!", "danger");
        }
    });
}

// Show alert messages dynamically
function showAlert(message, type) {
    let alertBox = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>`;
    $("#alertContainer").html(alertBox);
    setTimeout(() => $(".alert").fadeOut(500), 4000);
}
