$(document).ready(function() {
    // Search functionality
    $("#searchInput, #statusFilter, #dateFilter").on("keyup change", function() {
        filterLogs();
    });
    
    $("#searchButton").on("click", function() {
        filterLogs();
    });
    
    function filterLogs() {
        const searchText = $("#searchInput").val().toLowerCase();
        const statusFilter = $("#statusFilter").val();
        const dateFilter = $("#dateFilter").val();
        
        $(".maintenance-log-item").each(function() {
            const $item = $(this);
            const cardText = $item.text().toLowerCase();
            const isResolved = $item.data("resolved");
            const itemDate = new Date($item.data("date"));
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            // Status filter
            let statusMatch = true;
            if (statusFilter === "resolved") {
                statusMatch = isResolved === true;
            } else if (statusFilter === "pending") {
                statusMatch = isResolved === false;
            }
            
            // Date filter
            let dateMatch = true;
            if (dateFilter === "today") {
                const itemDateOnly = new Date(itemDate);
                itemDateOnly.setHours(0, 0, 0, 0);
                dateMatch = itemDateOnly.getTime() === today.getTime();
            } else if (dateFilter === "week") {
                const weekStart = new Date(today);
                weekStart.setDate(today.getDate() - today.getDay());
                dateMatch = itemDate >= weekStart;
            } else if (dateFilter === "month") {
                dateMatch = itemDate.getMonth() === today.getMonth() && 
                           itemDate.getFullYear() === today.getFullYear();
            }
            
            // Text search
            const textMatch = searchText === "" || cardText.includes(searchText);
            
            // Show/hide based on all filters
            if (statusMatch && dateMatch && textMatch) {
                $item.show();
            } else {
                $item.hide();
            }
        });
        
        // Always remove existing no-results message first
        $("#no-results-message").remove();
        
        // Show message if no results (count visible items excluding the no-results message)
        const visibleItems = $(".maintenance-log-item:visible").length;
        
        if (visibleItems === 0) {
            $("#maintenanceLogsList").append(
                '<div id="no-results-message" class="col-12"><div class="alert alert-info">No maintenance logs match your filters</div></div>'
            );
        }
    }
}); 