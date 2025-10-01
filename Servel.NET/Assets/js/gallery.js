"use strict";

const Gallery = (function() {
  const LAYOUT_MODES = ["contain", "fit-both", "fit-width", "clamp-width", "original"];
  const PLAYABLE_TEXT_EXTS = ["txt", "md"];

  let $body;
  let $gallery;
  let $overlayTitle;
  let $image;
  let $video;
  let $audio;
  let $document;
  let currentIndex;
  let layoutModeIndex = 0;

  async function renderText(url) {
    const response = await fetch(url);
    const responseText = await response.text();

    $document.srcdoc = `
      <!DOCTYPE html>
      <html>
        <head>
          <meta charset="utf-8">
          <title>Servel.NET</title>
          <link href="/_servel/img/favicon.png" rel="icon" type="image/png">
          <link href="/_servel/css/normalize.css" rel="stylesheet">
          <link href="/_servel/css/common.css" rel="stylesheet">
          <link href="/_servel/css/gallery_text.css" rel="stylesheet">
        </head>
        <body>
          <div id="content">${ume(responseText)}</div>
        </body>
      </html>`;
  }

  function clearContent() {
    $gallery.classList.remove("image", "video", "audio", "document");
    $image.removeAttribute("src");
    $video.removeAttribute("src");
    $video.removeAttribute("controls");
    $video.pause();
    $audio.removeAttribute("src");
    $audio.pause();
    $document.removeAttribute("src");
    $document.removeAttribute("srcdoc");
  }

  function setScrollable() {
    const value = ["fit-width", "clamp-width", "original", "document"].some((c) => $gallery.classList.contains(c));
    $gallery.classList.toggle("scrollable", value);
  }
  const isScrollable = () => $gallery.classList.contains("scrollable");

  function render() {
    clearContent();

    const entry = Entries.media[currentIndex];

    const url = entry.url;
    const type = entry.mediaType;

    document.title = `${entry.name} in ${decodeURIComponent(location.pathname)}`;
    $overlayTitle.innerHTML = HTMLSafe`<span class="icon">${entry.icon}</span> <a href="${entry.url}" target="_blank">${entry.name}</a>`;

    $gallery.classList.add(type);
    setScrollable();

    if(PLAYABLE_TEXT_EXTS.includes(entry.ext)) {
      renderText(url);
      return;
    }

    let $element;
    $element = $(`#${type}`);
    $element.src = url;
    $element.focus();

    window.scrollTo(0, 0);
    if(type == "video" || type == "audio") $element.play();

    //if(currentPage < imageUrls.length) (new Image()).src = imageUrls[currentPage];
  }

  function clamp(index) {
    if(index == null || isNaN(index) || index < 0) return 0;
    if(index >= Entries.media.length) return Entries.media.length - 1;
    return index;
  }

  function go(index) {
    const newIndex = clamp(index);
    if(newIndex == currentIndex) return;

    currentIndex = newIndex;
    render();
  }

  const prev = () => go(currentIndex - 1);
  const next = () => go(currentIndex + 1);
  const rewind = () => go(currentIndex - 10);
  const fastForward = () => go(currentIndex + 10);
  const jump = (url) => go(Entries.media.findIndex(entry => entry.url == url));

  function switchLayoutMode() {
    layoutModeIndex++;
    if(layoutModeIndex >= LAYOUT_MODES.length) layoutModeIndex = 0;
  }

  const showOverlay = () => $gallery.classList.add("overlay");
  const hideOverlay = () => $gallery.classList.remove("overlay");
  const isOverlayVisible = () => $gallery.classList.contains("overlay");

  function onSwiped(data) {
    if(!isVisible()) return;
    if(isScrollable() && data.target.closest("#content")) return;

    if(isOverlayVisible()) {
      if(data.dir == "down") hideOverlay();
      return;
    }

    if(data.dir == "up") showOverlay();
    if(data.dir == "left") next();
    if(data.dir == "right") prev();
  }

  function initTouchSwipes() {
    Common.detectTouchSwipes(onSwiped);
  }

  function initMouseSwipes() {
    $("#content-handle").addEventListener("mousedown", () => {
      $gallery.classList.add("content-handle-active");
      setTimeout(() => $gallery.classList.remove("content-handle-active"), 1000);
    });

    Common.detectMouseSwipes(onSwiped);
  }

  function initEvents() {
    if(document.body.classList.contains("touch")) initTouchSwipes();
    else initMouseSwipes();

    document.body.addEventListener("click", function(e) {
      if(!e.target) return;
      if(!isVisible()) return;

      else if(e.target.matches("#jump-listing, #overlay-jump-listing")) {
        e.preventDefault();
        Index.jumpListing();
      }
      else if(e.target.matches("#page-back")) {
        e.stopPropagation();
        prev();
      }
      else if(e.target.matches("#page-back-10")) {
        e.stopPropagation();
        rewind();
      }
      else if(e.target.matches("#page-next")) {
        e.stopPropagation();
        next();
      }
      else if(e.target.matches("#page-next-10")) {
        e.stopPropagation();
        fastForward();
      }
      else if(e.target.closest("#layout-mode")) {
        e.stopPropagation();
        switchLayoutMode();
        layout();
      }
    });

    window.addEventListener("keydown", function (e) {
      if(!isVisible()) return;

      if(e.keyCode == 39 || ((e.keyCode == 32 || e.keyCode == 13) && Common.atBottom())) {
        e.preventDefault();
        next();
      }
      else if(e.keyCode == 8 || e.keyCode == 37) {
        e.preventDefault();
        prev();
      }
    });

    $image.ondragstart = () => false;
    $video.ondragstart = () => false;
    $audio.ondragstart = () => false;

    const setLoop = (e) => e.target.loop = e.target.duration < (5 * 60);
    $video.addEventListener("loadedmetadata", setLoop);
    $audio.addEventListener("loadedmetadata", setLoop);

    $gallery.addEventListener("mouseenter", () => $video.toggleAttribute("controls", true));
    $gallery.addEventListener("mouseleave", () => $video.toggleAttribute("controls", false));
    $video.addEventListener("touchstart", (e) => e.target.matches("#video") && $video.toggleAttribute("controls", true));

    $document.addEventListener("load", () => $document.focus());
  }

  function layout() {
    const layoutMode = LAYOUT_MODES[layoutModeIndex];

    $gallery.classList.remove(...LAYOUT_MODES);
    $gallery.classList.add(layoutMode);
    setScrollable();
  }

  function onEntriesUpdate() {
    currentIndex = 0;
    document.body.classList.toggle("has-gallery", Entries.hasMedia);
    if(Entries.hasMedia && isVisible()) render();
  }

  function init() {
    $body = $("body");
    $gallery = $("#gallery");
    $overlayTitle = $("#overlay-title");
    $image = $("#image");
    $video = $("#video");
    $audio = $("#audio");
    $document = $("#document");
    $("#overlay-jump-listing").textContent = decodeURIComponent(location.pathname);

    initEvents();
    layout();
  }

  function show() {
    document.body.classList.add("gallery");
    render();
  }

  function hide() {
    document.body.classList.remove("gallery");
    clearContent();
    hideOverlay();
  }

  function isVisible() {
    return document.body.classList.contains("gallery");
  }

  return {
    init: init,
    show: show,
    hide: hide,
    isVisible: isVisible,
    onEntriesUpdate: onEntriesUpdate,
    go: go,
    prev: prev,
    next: next,
    rewind: rewind,
    fastForward: fastForward,
    jump: jump
  };
})();
