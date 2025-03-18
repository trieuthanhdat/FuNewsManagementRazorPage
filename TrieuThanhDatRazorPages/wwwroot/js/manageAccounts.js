$(document).ready(function () {
    console.log("manageAccounts.js Loaded Successfully!");

    if (typeof $ === "undefined") {
        console.error("Error: jQuery is not loaded. Please check your _Layout.cshtml.");
        return;
    }

    // Initialize SignalR connection
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/accountHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.start()
        .then(() => console.log("✅ Connected to SignalR!"))
        .catch(err => console.error("❌ SignalR Connection Error:", err.toString()));

    // Handle New Account Creation (Append to Table)
    connection.on("ReceiveAccountUpdate", function (newUser) {
        console.log("New Account Created! ID:", newUser.accountID);
        appendUserRow(newUser);
    });

    // Handle Account Update (Modify Specific Row)
    connection.on("ReceiveAccountUpdated", function (updatedUser) {
        console.log("Account Updated! ID:", updatedUser.accountID);
        updateUserRow(updatedUser);
    });

    // Handle Account Deletion (Remove Row)
    connection.on("ReceiveAccountDeleted", function (deletedUserID) {
        console.log("Account Deleted! ID:", deletedUserID);
        removeUserRow(deletedUserID);
    });

    //loadUsers(); // Initial load of users
});
function appendUserRow(user) {
    if (user == null) {
        console.warn("⚠️ Cannot append user: Invalid user data", user);
        return;
    }

    let tableBody = $("#userTable");
    let roleLabel = getRoleLabel(user.accountRole); // Convert role ID to text

    // Check if the row already exists to prevent duplicates
    if ($(`#userRow-${user.accountId}`).length > 0) {
        console.warn(`⚠️ User ID ${user.accountId} already exists. Skipping append.`);
        return;
    }

    let newRow = `
        <tr id="userRow-${user.accountId}">
            <td>${user.accountId}</td>
            <td class="user-name">${user.accountName}</td>
            <td class="user-email">${user.accountEmail}</td>
            <td class="user-role"><span class="badge bg-info">${roleLabel}</span></td>
            <td>
                <button class="btn btn-warning btn-sm" onclick="openEditModal(${user.accountId})">
                    <i class="fas fa-edit"></i> Edit
                </button>
                <button class="btn btn-danger btn-sm" onclick="confirmDelete(${user.accountId})">
                    <i class="fas fa-trash"></i> Delete
                </button>
            </td>
        </tr>
    `;

    tableBody.append(newRow).hide().fadeIn(300); // Smooth appearance
}
function updateUserRow(user) {
    if (!user || (!user.accountId && !user.AccountID)) {
        console.warn("⚠️ Cannot update user: Missing account ID", user);
        return;
    }
    let userID = user.accountId || user.AccountID; // Handle different casing
    let roleLabel = getRoleLabel(user.accountRole || user.AccountRole);
    let userRow = $(`#userRow-${userID}`);
    console.warn("On update user: account", user);

    if (userRow.length > 0) {
        console.log(`🔄 Updating User Row ID ${userID}`);
        userRow.fadeOut(200, function () {
            $(this).html(`
                <td>${userID}</td>
                <td class="user-name">${user.accountName || user.AccountName}</td>
                <td class="user-email">${user.accountEmail || user.AccountEmail}</td>
                <td class="user-role"><span class="badge bg-info">${roleLabel}</span></td>
                <td>
                    <button class="btn btn-warning btn-sm" onclick="openEditModal(${userID})">
                        <i class="fas fa-edit"></i> Edit
                    </button>
                    <button class="btn btn-danger btn-sm" onclick="confirmDelete(${userID})">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </td>
            `).fadeIn(300);
        });
    } else {
        console.warn(`⚠️ User ID ${userID} not found in the table. Appending instead.`);
        appendUserRow(user);
    }
}

function removeUserRow(userID) {
    if (!userID) {
        console.warn("⚠️ Cannot remove user: Invalid ID", userID);
        return;
    }

    let userRow = $(`#userRow-${userID}`);
    if (userRow.length > 0) {
        console.log(` Removing User Row ID ${userID}`);
        userRow.fadeOut(300, function () {
            $(this).remove();
        });
    } else {
        console.warn(`⚠️ User ID ${userID} not found in the table.`);
    }
}


$(document).ready(function () {
    $("#userForm").on("submit", function (event) {
        event.preventDefault(); // Prevent page reload

        let user = {
            AccountID: $("#userId").val() ? parseInt($("#userId").val()) : null, // Set to 0 for new users
            AccountName: $("#userName").val(),
            AccountEmail: $("#userEmail").val(),
            AccountRole: parseInt($("#userRole").val()),
            Password: $("#userPassword").val()
        };

        let antiForgeryToken = $("input[name='__RequestVerificationToken']").val();
        let apiUrl = user.AccountID ? "?handler=Edit" : "?handler=CreateUser"; // Different API for edit and create
        let httpMethod = "POST"; // Both actions use POST

        $.ajax({
            url: apiUrl,
            type: httpMethod,
            contentType: "application/json",
            headers: {
                "RequestVerificationToken": antiForgeryToken
            },
            data: JSON.stringify(user),
            success: function (response) {
                if (response.success) {
                    $("#userModal").modal("hide");
                    //loadUsers();
                    console.log("User Created Successfully!");
                   // console.log("Assigned AccountID:", response.data.accountID);
                } else {
                    alert(response.message);
                    console.error("Error creating user:", response.message);
                }
            },
            error: function (xhr) {
                console.error("Server error:", xhr.responseText);
                alert("Failed to create user.");
            }
        });
    });
});

// Fetch and load users into the table
function loadUsers() {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: "?handler=GetAllUsers",
        type: "GET",
        dataType: "json",
        headers: {
            "RequestVerificationToken": token // Pass CSRF token
        },
        success: function (data) {
            let tableBody = $("#userTable");
            tableBody.empty();
            if (!data.success) {
                console.error("❌ Error: " + response.message);
                alert(response.message);
                return;
            }
            $.each(data.data, function (index, user) {
                let roleLabel = getRoleLabel(user.accountRole);
                tableBody.append(`
                    <tr>
                        <td>${user.accountID}</td>
                        <td>${user.accountName}</td>
                        <td>${user.accountEmail}</td>
                        <td><span class="badge bg-info">${roleLabel}</span></td>
                        <td>
                            <button class="btn btn-warning btn-sm" onclick="openEditModal(${user.accountID})">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button class="btn btn-danger btn-sm" onclick="confirmDelete(${user.accountID})">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        </td>
                    </tr>
                `);
            });
        },
        error: function (xhr, status, error) {
            console.error("Error loading users:", error);
        }
    });
}

// Open modal for creating new user
function openCreateModal() {
    console.log("Opening Create Modal...");
    $("#modalTitle").text("Add User");
    $("#userId").val("");
    $("#userName").val("");
    $("#userEmail").val("");
    $("#userRole").val(1);
    $("#userPassword").val("");
    $("#userModal").modal("show");
}

function openEditModal(id) {
    console.log("Fetching User Data for Edit: ID", id);
    const token = $('input[name="__RequestVerificationToken"]').val();
    let urlApi = "?handler=GetUser";
    let httpMethod = "GET";
    $.ajax({
        url: urlApi,
        data: { id: id },
        type: httpMethod,
        dataType: "json",
        headers: {
            "RequestVerificationToken": token // Pass CSRF token
        },
        success: function (response) {
            console.log("User Data Received:", response);

            if (!response.success || !response.data) {
                console.error("❌ Error: " + response.message);
                alert(response.message);
                return;
            }

            let user = response.data;
            $("#modalTitle").text("Update User");

            setTimeout(() => {
                $("#userId").val(user.accountID);
                $("#userName").val(user.accountName);
                $("#userEmail").val(user.accountEmail);
                $("#userRole").val(user.accountRole);
                $("#userPassword").val(""); // Security: leave empty
                $("#userModal").modal("show");
            }, 100); // Delay in milliseconds

            //$("#userId").val(user.accountID);
            //$("#userName").val(user.accountName);
            //$("#userEmail").val(user.accountEmail);
            //$("#userRole").val(user.accountRole);
            //$("#userPassword").val(""); // Security: leave empty
            //$("#userModal").modal("show");
        },
        error: function (xhr, status, error) {
            console.error("❌ Server error:", xhr.responseText);
            alert("Failed to fetch user data.");
        }
    });
}

// Confirm delete user
function confirmDelete(userID) {
    if (!confirm("Are you sure you want to delete this user?")) return;

    let antiForgeryToken = $("input[name='__RequestVerificationToken']").val();
    let httpMethod = "POST";
    $.ajax({
        url: `?handler=DeleteAccount&id=${userID}`,
        type: httpMethod,
        contentType: "application/json",
        headers: {
            "RequestVerificationToken": antiForgeryToken
        },
        success: function (response) {
            if (response.success) {
                console.log("User Deleted Successfully!");
            } else {
                alert(response.message);
                console.error("Error deleting user:", response.message);
            }
        },
        error: function (xhr) {
            console.error("Server error:", xhr.responseText);
            alert("Failed to delete user.");
        }
    });
}

// Search users dynamically
function searchUsers() {
    let query = $("#searchBox").val().toLowerCase();
    console.log("Searching Users:", query);

    $("#userTable tr").each(function () {
        let name = $(this).find("td:eq(1)").text().toLowerCase();
        let email = $(this).find("td:eq(2)").text().toLowerCase();
        $(this).toggle(name.includes(query) || email.includes(query));
    });
}

// Convert role ID to readable label
function getRoleLabel(role) {
    switch (role) {
        case 0: return "Admin";
        case 1: return "Staff";
        case 2: return "Lecturer";
        default: return "Unknown";
    }
}

// Generate Report Function
function generateReport() {
    if (confirm("Do you want to generate the user report?")) {
        let reportType = prompt("Enter 'excel' for Excel report or 'pdf' for PDF report:", "excel");
        if (!reportType) return;

        let apiUrl = reportType.toLowerCase() === "pdf" ? "?handler=GeneratePdfReport" : "?handler=GenerateExcelReport";

        fetch(apiUrl, {
            method: "POST",
        })
            .then(response => response.blob())
            .then(blob => {
                let link = document.createElement("a");
                link.href = window.URL.createObjectURL(blob);
                link.download = reportType.toLowerCase() === "pdf" ? "UserReport.pdf" : "UserReport.xlsx";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                console.log(`${reportType.toUpperCase()} report downloaded successfully!`);
            })
            .catch(error => console.error("❌ Error openEditModal report:", error));
    }
}
