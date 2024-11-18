"use strict";

class Query {
  method = "";
  direction = "";
  text = "";

  isDefault() {
    return this.method == window.defaultQuery.method
      && this.direction == window.defaultQuery.direction
      && this.text == window.defaultQuery.text;
  }
}

const Entries = (function () {
  //Lists
  let all;
  let media;

  let query;

  function runFilter(entries) {
    if(query.text == "") return entries;

    const term = query.text.toLowerCase();
    return entries.filter(entry => entry.name.toLowerCase().includes(term));
  }

  const nameCollator = new Intl.Collator(undefined, { numeric: true, sensitivity: "base" });

  function sortByType(a, b) {
    var aType = a.type.toLowerCase();
    var bType = b.type.toLowerCase();

    if(aType < bType) return -1;
    if(aType > bType) return 1;

    return 0;
  }

  function runSort(entries) {
    let sortFunction;
    if(query.method == "name") sortFunction = (a, b) => nameCollator.compare(a.name, b.name);
    else if(query.method == "mtime") sortFunction = (a, b) => b.mtime - a.mtime;
    else if(query.method == "size") sortFunction = (a, b) => b.size - a.size;
    else if(query.method == "type") sortFunction = sortByType;

    entries.sort(sortFunction);
    if(query.direction == "desc") entries.reverse();

    return entries;
  }

  function update() {
    const specialEntries = window.directoryEntry.specialEntries.slice();
    const directoryEntries = runSort(runFilter(window.directoryEntry.directories.slice()));
    const fileEntries = runSort(runFilter(window.directoryEntry.files.slice()));

    all = [].concat(specialEntries, directoryEntries, fileEntries);

    media = fileEntries.filter(entry => entry.mediaType);

    Gallery.onEntriesUpdate();
    Listing.onEntriesUpdate();
  }

  return {
    update: update,
    get all() {
      return all;
    },
    get media() {
      return media;
    },
    get hasMedia() {
      return media.length > 0;
    },
    get query() {
      return query;
    },
    set query(newQuery) {
      query = newQuery;
    }
  };
})();