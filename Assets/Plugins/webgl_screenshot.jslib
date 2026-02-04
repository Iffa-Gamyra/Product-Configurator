mergeInto(LibraryManager.library, {
    VShowroom_DownloadScreenshot: function(base64) {
        var base64Str = UTF8ToString(base64);
        var link = document.createElement('a');
        link.href = 'data:image/png;base64,' + base64Str;
        link.download = 'screenshot.png';
        link.click();
    },

     // Stub to satisfy some other native call: DownloadScreenshot(int,int,int) -> int
    DownloadScreenshot: function(a, b, c) {
        // No-op implementation, just return 0 so the symbol exists
        return 0;
    }
});