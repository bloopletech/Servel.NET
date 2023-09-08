"use strict";

var Listing = (function() {
  var $container;
  var perPage = 99;
  var currentIndex;
  var moreContent;
  var scrollDebounce = false;

  function escapeHTML(unsafe) {
    if(unsafe == null) return "";
    return unsafe.toString()
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  function HTMLSafe(pieces) {
    var result = pieces[0];
    var substitutions = [].slice.call(arguments, 1);
    for(var i = 0; i < substitutions.length; ++i) result += escapeHTML(substitutions[i]) + pieces[i + 1];
    return result;
  }

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

  function onScrolled() {
    if(atBottom() && moreContent) {
      currentIndex += perPage;
      if(currentIndex >= Entries.all().length) moreContent = false;
      render();
    }
    scrollDebounce = false;
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
    window.addEventListener("scroll", function(e) {
      if(!scrollDebounce) {
        scrollDebounce = true;
        setTimeout(onScrolled, 0);
      }
    });

    document.body.addEventListener("click", function(e) {
      if(!e.target) return;

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
  }

  function onEntriesUpdate() {
    $container.innerHTML = "";
    currentIndex = 0;
    moreContent = true;
    render();
  }

  function init() {
    $container = $("#listing-container");

    onEntriesUpdate();

    initEvents();
  }

  return {
    init: init,
    onEntriesUpdate: onEntriesUpdate
  };
})();

window.addEventListener("DOMContentLoaded", Listing.init);