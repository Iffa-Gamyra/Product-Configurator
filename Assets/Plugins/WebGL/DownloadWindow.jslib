mergeInto(LibraryManager.library, {
  DownloadFile: function (url) {
    window.open(UTF8ToString(url), '_self');
  }
});
