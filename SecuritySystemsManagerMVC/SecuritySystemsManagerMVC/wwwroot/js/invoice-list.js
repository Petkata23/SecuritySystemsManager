$(document).ready(function() {
    // Initialize filter values from server
    if (searchTermFilter) {
        $("#searchInput").val(searchTermFilter);
    }
    if (paymentStatusFilter) {
        $("#paymentStatusFilter").val(paymentStatusFilter);
    }

    // AJAX filter functionality
    $("#paymentStatusFilter").on("change", function() {
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
        const paymentStatus = $("#paymentStatusFilter").val();
        const searchTerm = $("#searchInput").val();
        
        // Show loading indicator
        $("#invoicesTableContainer").html('<div class="text-center py-5"><i class="bi bi-hourglass-split fa-spin" style="font-size: 2rem;"></i><p class="mt-2">Loading...</p></div>');
        
        // Build the URL with filter parameters
        let url = '/Invoice/GetInvoicesPartial?';
        const params = new URLSearchParams();
        
        if (paymentStatus) params.append('paymentStatus', paymentStatus);
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
                $("#invoicesTableContainer").html(response);
            },
            error: function() {
                $("#invoicesTableContainer").html('<div class="text-center py-5"><i class="bi bi-exclamation-triangle text-danger" style="font-size: 2rem;"></i><p class="mt-2 text-danger">Error loading invoices. Please try again.</p></div>');
            }
        });
    }

    // Handle pagination clicks with AJAX
    $(document).on('click', '.pagination .page-link', function(e) {
        e.preventDefault();
        
        const pageNumber = $(this).attr("onclick")?.match(/loadInvoicesPage\((\d+)\)/)?.[1];
        if (pageNumber) {
            loadInvoicesPage(parseInt(pageNumber));
        }
    });
});

function loadInvoicesPage(pageNumber) {
    const paymentStatus = $("#paymentStatusFilter").val();
    const searchTerm = $("#searchInput").val();
    
    // Show loading indicator
    $("#invoicesTableContainer").html('<div class="text-center py-5"><i class="bi bi-hourglass-split fa-spin" style="font-size: 2rem;"></i><p class="mt-2">Loading...</p></div>');
    
    // Build the URL with filter parameters
    let url = '/Invoice/GetInvoicesPartial?';
    const params = new URLSearchParams();
    
    if (paymentStatus) params.append('paymentStatus', paymentStatus);
    if (searchTerm) params.append('searchTerm', searchTerm);
    
    // Add pagination parameters
    params.append('pageSize', '10');
    params.append('pageNumber', pageNumber.toString());
    
    url += params.toString();
    
    // Make AJAX request
    $.ajax({
        url: url,
        type: 'GET',
        success: function(response) {
            $("#invoicesTableContainer").html(response);
        },
        error: function() {
            $("#invoicesTableContainer").html('<div class="text-center py-5"><i class="bi bi-exclamation-triangle text-danger" style="font-size: 2rem;"></i><p class="mt-2 text-danger">Error loading invoices. Please try again.</p></div>');
        }
    });
} 