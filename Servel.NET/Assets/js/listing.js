"use strict";

var Listing = (function() {
  var $directoryInfo;
  var $tableWrapper;
  var $tableHeaderWrapper;
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
          <a href="${file.href}" class="new-tab ${file.class}" target="_blank">🗗</a>
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

    const byMediaType = { video: 0, image: 0, audio: 0, text: 0 };
    for(const file of window.directoryEntry.files) byMediaType[file.mediaType]++;

    $directoryInfo.innerHTML = HTMLSafe`
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
    $tableWrapper.scrollIntoView();
  }

  function initEvents() {
    const loadMoreObserver = new IntersectionObserver(([e]) => {
      if(e.intersectionRatio <= 0) return;
      loadMore();
    });
    loadMoreObserver.observe($("#load-more-spy"));

    const tableHeaderObserver = new IntersectionObserver(
      ([e]) => $tableHeaderWrapper.classList.toggle("pinned", e.intersectionRatio < 1),
      { threshold: [1] }
    );
    tableHeaderObserver.observe($("#table-header-wrapper-spy"));

    document.body.addEventListener("click", function(e) {
      if(!e.target) return;
      if(!isVisible()) return;

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

    Common.enableDragScroll($directoryInfo);
  }

  function onEntriesUpdate() {
    $container.innerHTML = "";
    currentIndex = 0;
    moreContent = true;
    render();
    renderInfo();
  }

  function init() {
    $directoryInfo = $("#directory-info");
    $tableWrapper = $("#table-wrapper");
    $tableHeaderWrapper = $("#table-header-wrapper");
    $container = $("#listing-container");
    const title = `Listing of ${decodeURIComponent(location.pathname)}`;
    $("#title").textContent = title;
    document.title = title;

    initEvents();
  }

  function show() {
    document.body.classList.add("listing");
    document.title = `Listing of ${decodeURIComponent(location.pathname)}`;
  }

  function hide() {
    document.body.classList.remove("listing");
  }

  function isVisible() {
    return document.body.classList.contains("listing");
  }

  return {
    init: init,
    show: show,
    hide: hide,
    isVisible: isVisible,
    onEntriesUpdate: onEntriesUpdate
  };
})();
