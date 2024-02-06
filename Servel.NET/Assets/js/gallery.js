"use strict";

var Gallery = (function() {
  const LAYOUT_MODES = ["contain", "fit-both", "fit-width", "clamp-width", "original"];

  let $body;
  let $gallery;
  let $image;
  let $video;
  let $audio;
  let currentIndex;
  let layoutModeIndex = 0;

  function renderText(url) {
    const http = new XMLHttpRequest();
    http.open("GET", url);
    http.onload = function() {
      $("#text-content").innerHTML = ume(http.responseText);
      $("#text").scrollTop = 0;
      $("#text-anchor").focus();
    }
    http.send();
  }

  function clearContent() {
    $gallery.classList.remove("image", "video", "audio", "text");
    $image.removeAttribute("src");
    $video.removeAttribute("src");
    $video.removeAttribute("controls");
    $video.pause();
    $audio.removeAttribute("src");
    $audio.pause();
    $("#text-content").innerHTML = "";
  }

  function render() {
    clearContent();

    const entry = Entries.media()[currentIndex];

    const url = entry.href;
    const type = entry.mediaType;

    $gallery.classList.add(type);

    let $element;
    if(type == "text") {
      renderText(url);
    }
    else {
      $element = $("#" + type);
      $element.src = url;
      $element.focus();
    }

    window.scrollTo(0, 0);
    if(type == "video" || type == "audio") $element.play();

    //if(currentPage < imageUrls.length) (new Image()).src = imageUrls[currentPage];
  }

  function clamp(index) {
    if(index == null || isNaN(index) || index < 0) return 0;
    if(index >= Entries.media().length) return Entries.media().length - 1;
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
  const jump = (url) => go(Entries.media().findIndex(entry => entry.href == url));

  function switchLayoutMode() {
    layoutModeIndex++;
    if(layoutModeIndex >= LAYOUT_MODES.length) layoutModeIndex = 0;
  }

  function onResize() {
    const vw = document.documentElement.clientWidth;
    const vh = document.documentElement.clientHeight;

    const viewportOrientation = vw > vh ? "landscape" : "portrait";
    $body.classList.remove("landscape", "portrait", "touch");
    $body.classList.add(viewportOrientation);
    if(navigator.maxTouchPoints > 0) $body.classList.add("touch");
  }

  function initEvents() {
    window.addEventListener("resize", onResize);
    onResize();

    document.body.addEventListener("click", function(e) {
      if(!e.target) return;
      if(!isVisible()) return;

      if(e.target.matches("#page-back, #content-page-back")) {
        e.stopPropagation();
        prev();
      }
      else if(e.target.matches("#page-back-10")) {
        e.stopPropagation();
        rewind();
      }
      else if(e.target.matches("#page-next, #content-page-next")) {
        e.stopPropagation();
        next();
      }
      else if(e.target.matches("#page-next-10")) {
        e.stopPropagation();
        fastForward();
      }
      else if(e.target.matches("#page-jump-listing")) {
        e.stopPropagation();
        Index.jumpListing();
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

    const setLoop = (e) => e.target.loop = e.target.duration < (5 * 60);
    $video.addEventListener("loadedmetadata", setLoop);
    $audio.addEventListener("loadedmetadata", setLoop);

    $video.addEventListener("mouseenter", () => $video.toggleAttribute("controls", true));
    $video.addEventListener("mouseleave", () => $video.toggleAttribute("controls", false));
    $video.addEventListener("touchstart", (e) => e.target.matches("#video") && $video.toggleAttribute("controls", true));
  }

  function layout() {
    const layoutMode = LAYOUT_MODES[layoutModeIndex];

    $gallery.classList.remove(...LAYOUT_MODES);
    $gallery.classList.add(layoutMode);
  }

  function onEntriesUpdate() {
    currentIndex = 0;
    document.body.classList.toggle("has-gallery", Entries.hasMedia());
    if(Entries.hasMedia() && isVisible()) render();
  }

  function init() {
    $body = $("body");
    $gallery = $("#gallery");
    $image = $("#image");
    $video = $("#video");
    $audio = $("#audio");

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
