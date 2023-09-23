"use strict";

var Entries = (function () {
  const IMAGE_EXTS = ["jpg", "jpeg", "png", "gif"];
  const VIDEO_EXTS = ["webm", "mp4", "mkv"];
  const AUDIO_EXTS = ["mp3", "m4a", "wav"];
  const TEXT_EXTS = ["txt"];

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

  function getIcon(entry) {
    if(entry.homeEntry) return "🏠";
    if(entry.topEntry) return "🔝";
    if(entry.parentEntry) return "⬆️";
    if(entry.directory) return "📁";
    if(entry.file) {
      if(entry.mediaType == "video") return "🎞️";
      if(entry.mediaType == "image") return "🖼️";
      if(entry.mediaType == "audio") return "🔊";
      return "📝";
    }

    return "";
  }

  function getType(entry) {
    if(entry.directory) return "Dir";
    return entry.name.slice((Math.max(0, entry.name.lastIndexOf(".")) || Infinity) + 1);
  }

  function getMediaType(entry) {
    if(entry.directory || !entry.type) return null;

    if(IMAGE_EXTS.includes(entry.type)) return "image";
    if(VIDEO_EXTS.includes(entry.type)) return "video";
    if(AUDIO_EXTS.includes(entry.type)) return "audio";
    if(TEXT_EXTS.includes(entry.type)) return "text";

    return null;
  }

  function getClass(entry) {
    const classes = [];
    if(entry.directory) classes.push("directory");
    if(entry.file) classes.push("file");
    if(entry.homeEntry) classes.push("home");
    if(entry.topEntry) classes.push("top");
    if(entry.parentEntry) classes.push("parent");
    if(entry.mediaType) classes.push("media", entry.mediaType);
    return classes.join(" ");
  }

  function inflateEntries(entries) {
    for(const entry of entries) {
      entry.type = getType(entry);
      entry.mediaType = getMediaType(entry);
      entry.icon = getIcon(entry);
      entry.sizeText = entry.file ? filesize(entry.size || 0, { base: 2, standard: "jedec" }) : "-";
      entry.mtimeText = entry.mtime != null ? f(new Date(entry.mtime)) : "-";
      entry.class = getClass(entry);
    }
  }

  function inflate() {
    for(const entry of window.directoryEntry.specialEntries) entry.directory = true;
    for(const entry of window.directoryEntry.directories) entry.directory = true;
    for(const entry of window.directoryEntry.files) entry.file = true;
    inflateEntries(window.directoryEntry.specialEntries);
    inflateEntries(window.directoryEntry.directories);
    inflateEntries(window.directoryEntry.files);
  }

  function updateLists() {
    const specialEntries = window.directoryEntry.specialEntries.slice();
    const directoryEntries = runSort(runFilter(window.directoryEntry.directories.slice()));
    const fileEntries = runSort(runFilter(window.directoryEntry.files.slice()));

    all = [].concat(specialEntries, directoryEntries, fileEntries);

    media = fileEntries.filter(entry => entry.mediaType);

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

  inflate();
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