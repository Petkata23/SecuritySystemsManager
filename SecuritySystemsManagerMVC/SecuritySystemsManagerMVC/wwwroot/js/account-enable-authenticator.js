window.addEventListener('load', function() {
    try {
        new QRCode(document.getElementById("qrCode"), {
            text: authenticatorUri,
            width: 200,
            height: 200,
            colorDark: "#000000",
            colorLight: "#ffffff",
            correctLevel: QRCode.CorrectLevel.H
        });
    } catch (e) {
        console.error("Error creating QR code:", e);
        document.getElementById("qrCode").innerHTML = 
            "<p class='text-danger'>Error generating QR code. Please try refreshing the page.</p>";
    }
}); 