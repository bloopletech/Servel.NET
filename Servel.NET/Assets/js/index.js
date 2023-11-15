"use strict";

var Index = (function() {
  function jumpListing() {
    Gallery.hide();
    Listing.show();
  }

  function jumpGallery() {
    Listing.hide();
    Gallery.show();
  }

  async function init() {
    const response = await fetch(window.location.href, {
      headers: {
        "Accept": "application/json",
      }
    });

    const directoryEntry = await response.json();
    inflateDirectoryEntry(directoryEntry);
    window.directoryEntry = directoryEntry;

    Listing.init();
    Gallery.init();
    Entries.update();
    jumpListing();
  }

  return {
    init: init,
    jumpListing: jumpListing,
    jumpGallery: jumpGallery
  };
})();

window.addEventListener("DOMContentLoaded", Index.init);