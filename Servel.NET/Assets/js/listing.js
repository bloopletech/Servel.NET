﻿"use strict";

var Listing = (function() {
  var $container;
  var perPage = 99;
  var currentIndex;
  var moreContent;

  function renderRow(file) {
    return HTMLSafe`
      <tr>
        <td class="name text-selectable">
          <span class="icon">${file.icon}</span>
          <a href="${file.href}" class="default ${file.class}" data-url="${file.href}" data-type="${file.mediaType}">${file.name}</a>
        </td>
        <td class="new-tab">
          <a href="${file.href}" class="new-tab ${file.class}" target="_blank">(New tab)</a>
        </td>
        <td class="type">${file.type}</td>
        <td class="size">${file.sizeText}</td>
        <td class="modified">${file.mtimeText}</td>
      </tr>
    `;
  }

  function renderTable(currentEntries) {
    var rows = currentEntries.map(function(entry) {
      return renderRow(entry);
    });

    return `
      <table>
        <tbody>
          ${rows.join("")}
        </tbody>
      </table>
    `;
  }

  function render() {
    var currentEntries = Entries.all().slice(currentIndex, currentIndex + perPage);
    $container.insertAdjacentHTML("beforeend", renderTable(currentEntries));
  }

  function renderInfo() {
    const directories = window.directoryEntry.directories.length;
    const files = window.directoryEntry.files.length;
    const byMediaType = Object.groupBy(window.directoryEntry.files, (entry) => entry.mediaType);
    for(const key of ["video", "image", "audio", "text"]) byMediaType[key] = byMediaType[key]?.length || 0;

    $("#directory-info").innerHTML = HTMLSafe`
      <div><span title="Items (Directories + Files)">⚪</span>\u2004${f(directories + files)}</div>
      <div><span title="Directories">📁</span>\u2004${f(directories)}</div>
      <div><span title="Files">📄</span>\u2004${f(files)}</div>
      <div><span title="Videos">🎞️</span>\u2004${f(byMediaType.video)}</div>
      <div><span title="Images">🖼️</span>\u2004${f(byMediaType.image)}</div>
      <div><span title="Audio">🔊</span>\u2004${f(byMediaType.audio)}</div>
      <div><span title="Text">📖</span>\u2004${f(byMediaType.text)}</div>
    `;
  }

  function loadMore() {
    if(!moreContent) return;

    currentIndex += perPage;
    if(currentIndex >= Entries.all().length) moreContent = false;
    render();
  }

  function applySort(sortable) {
    var previousSortable = $("th.sortable.sort-active");
    previousSortable.classList.remove("sort-active", "sort-asc", "sort-desc");

    if(sortable == previousSortable) {
      sortable.dataset.sortDirection = sortable.dataset.sortDirection == "asc" ? "desc" : "asc";
    }

    sortable.classList.add("sort-active", "sort-" + sortable.dataset.sortDirection);

    Entries.sort(sortable.dataset.sortMethod, sortable.dataset.sortDirection);
    sortable.scrollIntoView();
  }

  function initEvents() {
    const loadMoreObserver = new IntersectionObserver(entries => {
      if(entries[0].intersectionRatio <= 0) return;
      loadMore();
    });
    loadMoreObserver.observe($("#load-more-spy"));

    document.body.addEventListener("click", function(e) {
      if(!e.target) return;
      if(!Index.listingVisible()) return;

      if(e.target.matches("#jump-gallery")) {
        e.preventDefault();
        Index.jumpGallery();
      }
      else if(e.target.closest("th.sortable")) {
        e.preventDefault();
        applySort(e.target.closest("th.sortable"));
      }
      else if(e.target.matches("a.media:not(.new-tab)")) {
        e.preventDefault();
        Gallery.jump(e.target.dataset.url);
        Index.jumpGallery();
      }
    });

    $("#search").addEventListener("keyup", function(e) {
      e.stopPropagation();

      if(e.keyCode == 13) {
        Entries.filter($("#search").value);
      }
    });

    Common.enableDragScroll($("#directory-info"));
  }

  function onEntriesUpdate() {
    $container.innerHTML = "";
    currentIndex = 0;
    moreContent = true;
    render();
    renderInfo();
  }

  function init() {
    $container = $("#listing-container");
    const title = `Listing of ${decodeURIComponent(location.pathname)}`;
    $("#title").textContent = title;
    document.title = title;

    onEntriesUpdate();

    initEvents();
  }

  return {
    init: init,
    onEntriesUpdate: onEntriesUpdate
  };
})();
