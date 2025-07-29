function copyToClipboard(elementId) {
    var copyText = document.getElementById(elementId);
    copyText.select();
    copyText.setSelectionRange(0, 99999);
    document.execCommand("copy");
    alert("Copied to clipboard!");
}

$(document).ready(function() {
    $("#testRefreshBtn").click(function() {
        $("#testResult").html("<div class='spinner-border text-primary' role='status'><span class='sr-only'>Loading...</span></div>");
        
        $.ajax({
            url: '/DropboxAuth/TestTokenRefresh',
            type: 'GET',
            success: function(result) {
                if (result.success) {
                    $("#testResult").html("<div class='alert alert-success'>" + result.message + "<br>Token: " + result.tokenFirstChars + "</div>");
                } else {
                    $("#testResult").html("<div class='alert alert-danger'>" + result.message + "</div>");
                }
            },
            error: function(xhr, status, error) {
                $("#testResult").html("<div class='alert alert-danger'>Error: " + error + "</div>");
            }
        });
    });
}); 