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

  function init() {
    jumpListing();
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