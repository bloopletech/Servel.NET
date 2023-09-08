"use strict";

var Entries = (function() {
  //Lists
  var all;
  var media;
  var hasMedia;

  var sortMethod = "name";
  var sortDirection = "asc";
  var filterText = "";

  function runFilter(entries) {
    if(filterText == "") return entries;

    var filteredEntries = [];

    for(var i = 0; i < entries.length; i++) {
      var entry = entries[i];
      if(entry.name.toLowerCase().includes(filterText.toLowerCase())) filteredEntries.push(entry);
    }

    return filteredEntries;
  }

  var nameCollator = new Intl.Collator(undefined, { numeric: true, sensitivity: 'base' });

  function sortByName(a, b) {
    return nameCollator.compare(a.name, b.name);
  }

  function sortByMtime(a, b) {
    return b.mtime - a.mtime;
  }

  function sortBySize(a, b) {
    return b.size - a.size;
  }

  function sortByType(a, b) {
    var aType = a.type.toLowerCase();
    var bType = b.type.toLowerCase();

    if(aType < bType) return -1;
    if(aType > bType) return 1;

    return 0;
  }

  function runSort(entries) {
    var sortFunction;
    if(sortMethod == "name") sortFunction = sortByName;
    else if(sortMethod == "mtime") sortFunction = sortByMtime;
    else if(sortMethod == "size") sortFunction = sortBySize;
    else if(sortMethod == "type") sortFunction = sortByType;

    entries.sort(sortFunction);
    if(sortDirection == "desc") entries.reverse();

    return entries;
  }

  function updateLists() {
    var specialEntries = window.specialEntries.slice();
    var directoryEntries = runSort(runFilter(window.directoryEntries.slice()));
    var fileEntries = runSort(runFilter(window.fileEntries.slice()));

    all = [].concat(specialEntries, directoryEntries, fileEntries);

    media = [];
    for(var i = 0; i < fileEntries.length; i++) {
      if(fileEntries[i].media) media.push(fileEntries[i]);
    }

    hasMedia = media.length > 0;
  }

  function update() {
    updateLists();
    Gallery.onEntriesUpdate();
    Listing.onEntriesUpdate();
  }

  function sort(newSortMethod, newSortDirection) {
    sortMethod = newSortMethod;
    sortDirection = newSortDirection;
    update();
  }

  function filter(newFilterText) {
    filterText = newFilterText;
    update();
  }

  updateLists();

  return {
    all: function() {
      return all;
    },
    media: function() {
      return media;
    },
    hasMedia: function() {
      return hasMedia;
    },
    filter: filter,
    sort: sort
  };
})();