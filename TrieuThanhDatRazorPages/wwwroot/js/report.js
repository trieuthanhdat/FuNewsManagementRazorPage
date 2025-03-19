
function generateReport(type) {
    let apiUrl = type === "pdf" ? "?handler=GeneratePdfReport" : "?handler=GenerateExcelReport";

    fetch(apiUrl, { method: "POST" })
        .then(response => response.blob())
        .then(blob => {
            let link = document.createElement("a");
            link.href = window.URL.createObjectURL(blob);
            link.download = type === "pdf" ? "UserReport.pdf" : "UserReport.xlsx";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            console.log(` ${type.toUpperCase()} report downloaded successfully!`);
        })
        .catch(error => console.error("❌ Error generating report:", error));
}
function filterReports() {
    let startDate = document.getElementById("startDate").value;
    let endDate = document.getElementById("endDate").value;
    let search = document.getElementById("searchBox").value.trim();

    $.ajax({
        url: "/Admin/Report?handler=GetReport",
        type: "GET",
        data: { startDate, endDate, search },
        success: function (response) {
            if (response.success) {
                updateReportTable(response.data);
            } else {
                $("#reportTable").html(`<tr><td colspan='5' class='text-center text-muted'>${response.message}</td></tr>`);
            }
        },
        error: function (xhr) {
            console.error("❌ Server error:", xhr.responseText);
            alert("Failed to fetch reports.");
        }
    });
}

function updateReportTable(data) {
    let tableBody = $("#reportTable");
    tableBody.empty();

    data.forEach(news => {
        let row = `
            <tr>
                <td>${news.newsArticleId}</td>
                <td class="title">${news.newsTitle}</td>
                <td class="headline">${news.headline}</td>
                <td>${news.createdDate}</td>
                <td class="author">${news.createdBy || "Unknown"}</td>
            </tr>
        `;
        tableBody.append(row);
    });
}

function searchReports() {
    let searchValue = document.getElementById("searchBox").value.toLowerCase();
    let rows = document.querySelectorAll("#reportTable tr");

    rows.forEach(row => {
        let title = row.querySelector(".title")?.innerText.toLowerCase() || "";
        let headline = row.querySelector(".headline")?.innerText.toLowerCase() || "";
        let author = row.querySelector(".author")?.innerText.toLowerCase() || "";

        if (title.includes(searchValue) || headline.includes(searchValue) || author.includes(searchValue)) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });

    // Show 'No results' message if all rows are hidden
    let visibleRows = Array.from(rows).some(row => row.style.display !== "none");
    document.getElementById("noResultsRow").style.display = visibleRows ? "none" : "";
}
var newsChart = null; 
// Load report and generate statistics
function loadReport() {
    let startDate = $("#startDate").val();
    let endDate = $("#endDate").val();

    $.ajax({
        url: "?handler=GetReport",
        type: "GET",
        data: { startDate: startDate, endDate: endDate },
        dataType: "json",
        success: function (response) {
            if (response.success) {
                let tableBody = $("#reportTable");
                tableBody.empty();

                let reportData = response.data;
                let dateCountMap = {}; // Store article count per date

                reportData.forEach(article => {
                    let createdDate = new Date(article.createdDate).toISOString().split('T')[0];

                    tableBody.append(`
                        <tr>
                            <td>${article.newsArticleId}</td>
                            <td>${article.newsTitle}</td>
                            <td>${article.headline}</td>
                            <td>${createdDate}</td>
                            <td>${article.createdBy || "Unknown"}</td>
                        </tr>
                    `);

                    // Count occurrences of articles by date
                    dateCountMap[createdDate] = (dateCountMap[createdDate] || 0) + 1;
                });

                // Convert dictionary to arrays
                let newsTitles = Object.keys(dateCountMap);
                let newsCounts = Object.values(dateCountMap);

                console.log("Updating Chart with:");
                console.log("Labels (Dates):", newsTitles);
                console.log("Data (Counts):", newsCounts);

                updateNewsChart(newsTitles, newsCounts);
            } else {
                alert("No data found for the selected period.");
            }
        },
        error: function (xhr) {
            console.error("Error loading report:", xhr.responseText);
        }
    });
}

// Function to update the chart
function updateNewsChart(labels, data) {
    let ctx = document.getElementById("newsChart").getContext("2d");

    if (newsChart) {
        newsChart.destroy(); // Destroy previous chart instance before creating a new one
    }

    newsChart = new Chart(ctx, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [{
                label: "Number of Articles",
                backgroundColor: "rgba(54, 162, 235, 0.6)",
                borderColor: "rgba(54, 162, 235, 1)",
                borderWidth: 1,
                data: data
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
}

function exportExcel() {
    let startDate = $("#startDate").val();
    let endDate = $("#endDate").val();
    window.location.href = `?handler=ExportExcel&startDate=${startDate}&endDate=${endDate}`;
}

function exportPdf() {
    let startDate = $("#startDate").val();
    let endDate = $("#endDate").val();
    window.location.href = `?handler=ExportPdf&startDate=${startDate}&endDate=${endDate}`;
}

// Load report on page load (default last 7 days)
$(document).ready(function () {
    loadReport();
});
