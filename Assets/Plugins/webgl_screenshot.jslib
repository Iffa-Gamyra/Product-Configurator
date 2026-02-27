mergeInto(LibraryManager.library, {
    VShowroom_DownloadScreenshot: function(base64) {
        var base64Str = UTF8ToString(base64);
        var link = document.createElement('a');
        link.href = 'data:image/png;base64,' + base64Str;
        link.download = 'screenshot.png';
        link.click();
    },
     VShowroom_DownloadPDFUrl: function(urlPtr, filenamePtr) {
        var url = UTF8ToString(urlPtr);
        var filename = UTF8ToString(filenamePtr);

        var link = document.createElement('a');
        link.href = url;
        link.download = filename || "brochure.pdf";
        link.target = "_blank";
        link.rel = "noopener";

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },

     // Stub to satisfy some other native call: DownloadScreenshot(int,int,int) -> int
    DownloadScreenshot: function(a, b, c) {
        // No-op implementation, just return 0 so the symbol exists
        return 0;
    }
});