"use strict";

const Listing = (function () {
  let $listing;
  let $directoryInfo;
  let $toolbarSpy;
  let $toolbarWrapper;
  let $entries;
  let title;
  let hasThumbnails;

  function renderEntry(entry) {
    const thumbnailInner = hasThumbnails && entry.audioVideo ? HTMLSafe`<img src="${entry.url}?action=thumbnail">` : "";
    return HTMLSafe`
      <div class="entry">
        ${hasThumbnails ? HTMLSafe`<div class="thumbnail">${thumbnailInner}</div>` : ""}
        <div class="data">
          <div class="name text-selectable">
            <span class="icon">${entry.icon}</span>
            <a href="${entry.url}" class="default ${entry.class}" data-url="${entry.url}" data-type="${entry.mediaType}">${entry.name}</a>
          </div>
          <div class="modified">${entry.mtimeText}</div>
          <div class="size">${entry.sizeText}</div>
          <div class="type">${entry.type}</div>
        </div>
      </div>
    `;
  }

  function render() {
    $entries.innerHTML = Entries.all.map(entry => renderEntry(entry)).join("");
  }

  function renderInfo() {
    const directories = window.directory.directories.length;
    const files = window.directory.files.length;

    const byMediaType = { video: 0, image: 0, audio: 0, document: 0 };
    for(const file of window.directory.files) byMediaType[file.mediaType]++;

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

  function reflectSort(sortable) {
    const previousSortable = $(".sortable.sort-active");
    if(previousSortable) previousSortable.classList.remove("sort-active", "sort-asc", "sort-desc");

    if(sortable == previousSortable) {
      sortable.dataset.sortDirection = sortable.dataset.sortDirection == "asc" ? "desc" : "asc";
    }

    sortable.classList.add("sort-active", `sort-${sortable.dataset.sortDirection}`);
  }

  function onSort(sortable) {
    reflectSort(sortable);

    Entries.query.method = sortable.dataset.sortMethod;
    Entries.query.direction = sortable.dataset.sortDirection;
    saveQuery();
    Entries.update();
  }

  function reflectLayoutMode(element) {
    const previousElement = $(".layout-mode.active");
    if(previousElement) previousElement.classList.remove("active");

    element.classList.add("active");
  }

  function onLayoutMode(element) {
    reflectLayoutMode(element);

    Entries.query.layoutMode = element.dataset.layoutMode;
    saveQuery();
  }

  function openEntry(target) {
    const entry = target.closest(".entry");
    const link = entry.querySelector("a");
    link.click();
  }

  function initQuery() {
    const state = sessionStorage.getItem(location.pathname);
    if(state) Entries.query = Object.assign(new Query, JSON.parse(state));

    const sortable = $(`.sortable[data-sort-method="${Entries.query.method}"]`);
    sortable.dataset.sortDirection = Entries.query.direction;
    reflectSort(sortable);

    const element = $(`.layout-mode[data-layout-mode="${Entries.query.layoutMode}"]`);
    reflectLayoutMode(element);

    $("#search").value = Entries.query.text;
  }

  function saveQuery() {
    if(Entries.query.isDefault()) sessionStorage.removeItem(location.pathname);
    else sessionStorage.setItem(location.pathname, JSON.stringify(Entries.query));
  }

  function initEvents() {
    const toolbarObserver = new IntersectionObserver(
      ([e]) => $toolbarWrapper.classList.toggle("pinned", e.intersectionRatio < 1),
      { threshold: [1] }
    );
    toolbarObserver.observe($toolbarSpy);

    document.body.addEventListener("click", function(e) {
      if(!e.target) return;
      if(!isVisible()) return;

      if(e.target.matches("#jump-gallery")) {
        e.preventDefault();
        Index.jumpGallery();
      }
      else if(e.target.matches(".layout-mode")) {
        e.preventDefault();
        onLayoutMode(e.target);
        layout();
      }
      else if(e.target.matches(".sortable")) {
        e.preventDefault();
        onSort(e.target);
      }
      else if(e.target.closest("a.media")) {
        e.preventDefault();
        Gallery.jump(e.target.closest("a.media").dataset.url);
        Index.jumpGallery();
      }
      else if(e.target.closest(".thumbnail")) {
        e.preventDefault();
        openEntry(e.target);
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
    Common.enableDragScroll($("#toolbar"));
  }

  function layout() {
    $listing.classList.remove("list", "thumbnails");
    $listing.classList.add(Entries.query.layoutMode);
    $toolbarSpy.scrollIntoView();
  }

  function onEntriesUpdate() {
    hasThumbnails = !!(window.configuration.thumbnailsEnabled && Entries.hasAudioVideo);
    $listing.classList.toggle("has-thumbnails", hasThumbnails);

    if(!hasThumbnails && Entries.query.layoutMode == "thumbnails") onLayoutMode($(`.layout-mode[data-layout-mode="list"]`));

    layout();
    render();
    renderInfo();
  }

  function init() {
    $listing = $("#listing");
    $directoryInfo = $("#directory-info");
    $toolbarSpy = $("#toolbar-spy");
    $toolbarWrapper = $("#toolbar-wrapper");
    $entries = $("#entries");
    title = decodeURIComponent(location.pathname);
    $("#title").textContent = `📁 ${decodeURIComponent(location.pathname)}`;
    document.title = title;

    initQuery();
    initEvents();
  }

  function show() {
    document.body.classList.add("listing");
    document.title = title;
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
