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
            console.log(`✅ ${type.toUpperCase()} report downloaded successfully!`);
        })
        .catch(error => console.error("❌ Error generating report:", error));
}
