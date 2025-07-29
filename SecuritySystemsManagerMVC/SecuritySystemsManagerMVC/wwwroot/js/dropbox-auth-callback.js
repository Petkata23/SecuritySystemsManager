function copyToClipboard(elementId) {
    var copyText = document.getElementById(elementId);
    copyText.select();
    copyText.setSelectionRange(0, 99999);
    document.execCommand("copy");
    alert("Copied to clipboard!");
}

$(document).ready(function() {
    $('#testRefreshBtn').click(function() {
        $(this).prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Testing...');
        $('#refreshResult').html('');
        
        $.ajax({
            url: '/DropboxAuth/TestTokenRefresh',
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    $('#refreshResult').html('<div class="alert alert-success">' + 
                        '<strong>Success!</strong> ' + response.message + '<br>' +
                        'Token: ' + response.tokenFirstChars + '</div>');
                } else {
                    $('#refreshResult').html('<div class="alert alert-danger">' + 
                        '<strong>Error!</strong> ' + response.message + '</div>');
                }
            },
            error: function() {
                $('#refreshResult').html('<div class="alert alert-danger">' + 
                    '<strong>Error!</strong> Failed to test token refresh.</div>');
            },
            complete: function() {
                $('#testRefreshBtn').prop('disabled', false).html('Test Token Refresh');
            }
        });
    });
}); 