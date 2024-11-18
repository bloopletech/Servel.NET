"use strict";

const Index = (function() {
  function jumpListing() {
    window.location.hash = "";
  }

  function jumpGallery() {
    window.location.hash = "#gallery";
  }

  function openListing() {
    Gallery.hide();
    Listing.show();
  }

  function openGallery() {
    Listing.hide();
    Gallery.show();
  }

  function onHashChange() {
    const hash = window.location.hash.substring(1);
    if(hash == "gallery") openGallery();
    else if(hash == "") openListing();
  }

  async function init() {
    window.addEventListener("hashchange", onHashChange);

    const response = await fetch(window.location.href, {
      headers: {
        "Accept": "application/json",
      }
    });

    const { directory, defaultQuery } = await response.json();
    inflateDirectory(directory);
    window.directory = directory;
    window.defaultQuery = Object.assign(new Query, defaultQuery ?? { method: "name", direction: "asc", text: "" });
    Entries.query = Object.assign(new Query, window.defaultQuery);

    Listing.init();
    Gallery.init();
    Entries.update();
    onHashChange();
  }

  return {
    init: init,
    jumpListing: jumpListing,
    jumpGallery: jumpGallery
  };
})();

window.addEventListener("DOMContentLoaded", Index.init);