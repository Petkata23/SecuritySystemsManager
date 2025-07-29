$(document).ready(function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // View toggle functionality
    $("#tableViewBtn").click(function() {
        $(this).addClass('active');
        $("#cardViewBtn").removeClass('active');
        $("#tableView").removeClass('d-none');
        $("#cardView").addClass('d-none');
    });
    
    $("#cardViewBtn").click(function() {
        $(this).addClass('active');
        $("#tableViewBtn").removeClass('active');
        $("#tableView").addClass('d-none');
        $("#cardView").removeClass('d-none');
    });
    
    // Filter functionality
    $("#statusFilter, #startDate, #endDate").on("change", filterOrders);
    $("#searchBtn").on("click", filterOrders);
    $("#searchInput").on("keyup", function(e) {
        if (e.key === "Enter") {
            filterOrders();
        }
    });
    
    function filterOrders() {
        const status = $("#statusFilter").val().toLowerCase();
        const startDate = $("#startDate").val();
        const endDate = $("#endDate").val();
        const searchText = $("#searchInput").val().toLowerCase();
        
        // Filter table view
        $("#ordersTable tbody tr").each(function() {
            filterElement($(this), status, startDate, endDate, searchText);
        });
        
        // Filter card view
        $(".order-card-col").each(function() {
            filterElement($(this), status, startDate, endDate, searchText);
        });
        
        // Show empty message if all rows are hidden in the active view
        const activeView = $("#tableView").hasClass("d-none") ? "card" : "table";
        
        if (activeView === "table") {
            const visibleRows = $("#ordersTable tbody tr:visible").length;
            if (visibleRows === 0) {
                if ($("#no-results-row").length === 0) {
                    $("#ordersTable tbody").append(
                        '<tr id="no-results-row"><td colspan="6" class="text-center py-4">' +
                        '<i class="bi bi-search text-muted mb-2" style="font-size: 2rem;"></i>' +
                        '<p class="mb-0">No orders match your search criteria</p>' +
                        '</td></tr>'
                    );
                }
            } else {
                $("#no-results-row").remove();
            }
        } else {
            const visibleCards = $(".order-card-col:visible").length;
            if (visibleCards === 0) {
                if ($("#no-results-card").length === 0) {
                    $("#cardView .row").append(
                        '<div id="no-results-card" class="col-12 text-center py-5">' +
                        '<i class="bi bi-search text-muted mb-2" style="font-size: 2rem;"></i>' +
                        '<p class="mb-0">No orders match your search criteria</p>' +
                        '</div>'
                    );
                }
            } else {
                $("#no-results-card").remove();
            }
        }
    }
    
    function filterElement(element, status, startDate, endDate, searchText) {
        const rowStatus = element.data("status").toString().toLowerCase();
        const rowDate = element.data("date");
        const rowText = element.text().toLowerCase();
        
        let statusMatch = status === "" || rowStatus.includes(status);
        let dateMatch = true;
        
        if (startDate && rowDate < startDate) {
            dateMatch = false;
        }
        
        if (endDate && rowDate > endDate) {
            dateMatch = false;
        }
        
        let textMatch = searchText === "" || rowText.includes(searchText);
        
        if (statusMatch && dateMatch && textMatch) {
            element.show();
        } else {
            element.hide();
        }
    }
}); 