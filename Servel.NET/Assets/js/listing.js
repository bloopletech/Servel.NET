"use strict";

const Listing = (function() {
  let $directoryInfo;
  let $tableWrapper;
  let $tableHeaderWrapper;
  let $container;
  const perPage = 99;
  let currentIndex;
  let moreContent;

  function renderRow(file) {
    return HTMLSafe`
      <tr>
        <td class="name text-selectable">
          <span class="icon">${file.icon}</span>
          <a href="${file.href}" class="default ${file.class}" data-url="${file.href}" data-type="${file.mediaType}">${file.name}</a>
        </td>
        <td class="type">${file.type}</td>
        <td class="size">${file.sizeText}</td>
        <td class="modified">${file.mtimeText}</td>
      </tr>
    `;
  }

  function renderTable(currentEntries) {
    return `
      <table>
        <tbody>
          ${currentEntries.map(entry => renderRow(entry)).join("")}
        </tbody>
      </table>
    `;
  }

  function render() {
    const currentEntries = Entries.all.slice(currentIndex, currentIndex + perPage);
    $container.insertAdjacentHTML("beforeend", renderTable(currentEntries));
  }

  function renderInfo() {
    const directories = window.directoryEntry.directories.length;
    const files = window.directoryEntry.files.length;

    const byMediaType = { video: 0, image: 0, audio: 0, document: 0 };
    for(const file of window.directoryEntry.files) byMediaType[file.mediaType]++;

    $directoryInfo.innerHTML = HTMLSafe`
      <div><span title="Items (Directories + Files)">⚪</span>\u2004${f(directories + files)}</div>
      <div><span title="Directories">📁</span>\u2004${f(directories)}</div>
      <div><span title="Files">📄</span>\u2004${f(files)}</div>
      <div><span title="Videos">🎞️</span>\u2004${f(byMediaType.video)}</div>
      <div><span title="Images">🖼️</span>\u2004${f(byMediaType.image)}</div>
      <div><span title="Audio">🔊</span>\u2004${f(byMediaType.audio)}</div>
      <div><span title="Documents">📝</span>\u2004${f(byMediaType.document)}</div>
    `;
  }

  function loadMore() {
    if(!moreContent) return;

    currentIndex += perPage;
    if(currentIndex >= Entries.all.length) moreContent = false;
    render();
  }

  function applySort(sortable) {
    const previousSortable = $("th.sortable.sort-active");
    if(previousSortable) previousSortable.classList.remove("sort-active", "sort-asc", "sort-desc");

    if(sortable == previousSortable) {
      sortable.dataset.sortDirection = sortable.dataset.sortDirection == "asc" ? "desc" : "asc";
    }

    sortable.classList.add("sort-active", `sort-${sortable.dataset.sortDirection}`);
  }

  function onSort(sortable) {
    applySort(sortable);

    Entries.query.method = sortable.dataset.sortMethod;
    Entries.query.direction = sortable.dataset.sortDirection;
    saveQuery();
    Entries.update();
    $tableWrapper.scrollIntoView();
  }

  function initQuery() {
    const state = sessionStorage.getItem(location.pathname);
    if(state) Entries.query = Object.assign(new Query, JSON.parse(state));

    const sortable = $(`th.sortable[data-sort-method="${Entries.query.method}"]`);
    sortable.dataset.sortDirection = Entries.query.direction;
    applySort(sortable);

    $("#search").value = Entries.query.text;
  }

  function saveQuery() {
    if(Entries.query.isDefault()) sessionStorage.removeItem(location.pathname);
    else sessionStorage.setItem(location.pathname, JSON.stringify(Entries.query));
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
        onSort(e.target.closest("th.sortable"));
      }
      else if(e.target.matches("a.media")) {
        e.preventDefault();
        Gallery.jump(e.target.dataset.url);
        Index.jumpGallery();
      }
    });

    $("#search").addEventListener("keyup", function(e) {
      e.stopPropagation();

      if(e.keyCode == 13) {
        Entries.query.text = $("#search").value;
        saveQuery();
        Entries.update();
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

    initQuery();
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
