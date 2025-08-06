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
    
    // AJAX filter functionality
    $("#statusFilter, #startDate, #endDate").on("change", function() {
        applyServerSideFilters();
    });
    
    $("#searchBtn").on("click", function() {
        applyServerSideFilters();
    });
    
    $("#searchInput").on("keyup", function(e) {
        if (e.key === "Enter") {
            applyServerSideFilters();
        }
    });
    
    function applyServerSideFilters() {
        const status = $("#statusFilter").val();
        const startDate = $("#startDate").val();
        const endDate = $("#endDate").val();
        const searchTerm = $("#searchInput").val();
        
        // Show loading indicator
        $("#ordersTableContainer").html('<div class="text-center py-5"><i class="bi bi-hourglass-split fa-spin" style="font-size: 2rem;"></i><p class="mt-2">Loading...</p></div>');
        
        // Build the URL with filter parameters
        let url = '/SecuritySystemOrder/GetOrdersPartial?';
        const params = new URLSearchParams();
        
        if (status) params.append('status', status);
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        if (searchTerm) params.append('searchTerm', searchTerm);
        
        // Add pagination parameters (start with page 1 when filtering)
        params.append('pageSize', '10');
        params.append('pageNumber', '1');
        
        url += params.toString();
        
        // Make AJAX request
        $.ajax({
            url: url,
            type: 'GET',
            success: function(response) {
                $("#ordersTableContainer").html(response);
                
                // Re-initialize tooltips for new content
                var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
                    return new bootstrap.Tooltip(tooltipTriggerEl);
                });
                
                // Re-initialize view toggle functionality
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
            },
            error: function() {
                $("#ordersTableContainer").html('<div class="text-center py-5"><i class="bi bi-exclamation-triangle text-danger" style="font-size: 2rem;"></i><p class="mt-2 text-danger">Error loading orders. Please try again.</p></div>');
            }
        });
    }
    
    // Handle pagination clicks with AJAX
    $(document).on('click', '.pagination .page-link', function(e) {
        e.preventDefault();
        
        const href = $(this).attr('href');
        if (href) {
            // Extract parameters from href
            const url = new URL(href, window.location.origin);
            const searchTerm = url.searchParams.get('searchTerm') || '';
            const startDate = url.searchParams.get('startDate') || '';
            const endDate = url.searchParams.get('endDate') || '';
            const status = url.searchParams.get('status') || '';
            const pageNumber = url.searchParams.get('pageNumber') || '1';
            
            // Show loading indicator
            $("#ordersTableContainer").html('<div class="text-center py-5"><i class="bi bi-hourglass-split fa-spin" style="font-size: 2rem;"></i><p class="mt-2">Loading...</p></div>');
            
            // Build AJAX URL
            let ajaxUrl = '/SecuritySystemOrder/GetOrdersPartial?';
            const params = new URLSearchParams();
            
            if (searchTerm) params.append('searchTerm', searchTerm);
            if (startDate) params.append('startDate', startDate);
            if (endDate) params.append('endDate', endDate);
            if (status) params.append('status', status);
            params.append('pageNumber', pageNumber);
            params.append('pageSize', '10');
            
            ajaxUrl += params.toString();
            
            // Make AJAX request
            $.ajax({
                url: ajaxUrl,
                type: 'GET',
                success: function(response) {
                    $("#ordersTableContainer").html(response);
                    
                    // Re-initialize tooltips for new content
                    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
                        return new bootstrap.Tooltip(tooltipTriggerEl);
                    });
                    
                    // Re-initialize view toggle functionality
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
                },
                error: function() {
                    $("#ordersTableContainer").html('<div class="text-center py-5"><i class="bi bi-exclamation-triangle text-danger" style="font-size: 2rem;"></i><p class="mt-2 text-danger">Error loading orders. Please try again.</p></div>');
                }
            });
        }
    });
    
    // Initialize filters with current values from ViewBag (if any)
    $(document).ready(function() {
        // These values will be set by the server when returning filtered results
        if (typeof searchTermFilter !== 'undefined') {
            $("#searchInput").val(searchTermFilter);
        }
        if (typeof startDateFilter !== 'undefined') {
            $("#startDate").val(startDateFilter);
        }
        if (typeof endDateFilter !== 'undefined') {
            $("#endDate").val(endDateFilter);
        }
        if (typeof statusFilter !== 'undefined') {
            $("#statusFilter").val(statusFilter);
        }
    });
}); 