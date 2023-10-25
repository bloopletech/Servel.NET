"use strict";

var Index = (function() {
  function galleryVisible() {
    return document.body.classList.contains("gallery");
  }

  function listingVisible() {
    return document.body.classList.contains("listing");
  }

  function jumpListing() {
    document.body.classList.remove("gallery");
    document.body.classList.add("listing");
  }

  function jumpGallery() {
    document.body.classList.remove("listing");
    document.body.classList.add("gallery");
  }

  async function init() {
    jumpListing();

    const response = await fetch(window.location.href, {
      headers: {
        "Accept": "application/json",
      }
    });

    const directoryEntry = await response.json();
    inflateDirectoryEntry(directoryEntry);
    window.directoryEntry = directoryEntry;

    Entries.updateLists();
    Listing.init();
    Gallery.init();
  }

  return {
    init: init,
    galleryVisible: galleryVisible,
    listingVisible: listingVisible,
    jumpListing: jumpListing,
    jumpGallery: jumpGallery
  };
})();

window.addEventListener("DOMContentLoaded", Index.init);