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
    $video.removeAttribute('src');
    $video.pause();
    $audio.removeAttribute('src');
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

    if(Index.galleryVisible()) {
      window.scrollTo(0, 0);

      if(type == "video" || type == "audio") $element.play();
    }

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

  function prev() {
    go(currentIndex - 1);
  }

  function next() {
    go(currentIndex + 1);
  }

  function rewind() {
    go(currentIndex - 10);
  }

  function fastForward() {
    go(currentIndex + 10);
  }

  function jump(url) {
    const index = Entries.media().findIndex(function(entry) {
      return entry.href == url;
    });
    go(index);
  }

  function switchLayoutMode() {
    layoutModeIndex++;
    if(layoutModeIndex >= LAYOUT_MODES.length) layoutModeIndex = 0;
  }

  function playPauseVideo() {
    if ($video.paused || $video.ended) $video.play();
    else $video.pause();
  }

  function initEvents() {
    document.body.addEventListener("click", function(e) {
      if(!e.target) return;

      if(e.target.matches("#page-back")) {
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
      else if(e.target.matches("#page-jump-listing")) {
        e.stopPropagation();
        Index.jumpListing();
      }
      else if(e.target.closest("#layout-mode")) {
        e.stopPropagation();
        switchLayoutMode();
        layout();
      }
      else if(e.target.matches("#video")) {
        e.stopPropagation();
        playPauseVideo();
      }
    });

    window.addEventListener("keydown", function(e) {
      if(e.target == $("#search")) return;

      if(e.keyCode == 39 || ((e.keyCode == 32 || e.keyCode == 13) && Common.atBottom())) {
        e.preventDefault();
        next();
      }
      else if(e.keyCode == 8 || e.keyCode == 37) {
        e.preventDefault();
        prev();
      }
    });
  }

  function layout() {
    const vw = document.documentElement.clientWidth;
    const vh = document.documentElement.clientHeight;

    const viewportOrientation = vw > vh ? "landscape" : "portrait";
    $body.classList.remove("landscape", "portrait", "touch");
    $body.classList.add(viewportOrientation);
    if(navigator.maxTouchPoints > 0) $body.classList.add("touch");

    const layoutMode = LAYOUT_MODES[layoutModeIndex];

    $gallery.classList.remove(...LAYOUT_MODES);
    $gallery.classList.add(layoutMode);
  }

  function initLayout() {
    window.addEventListener("resize", layout);
    layout();
  }

  function onEntriesUpdate() {
    currentIndex = 0;
    if(Entries.hasMedia()) render();
  }

  function init() {
    $body = $("body");
    $gallery = $("#gallery");
    $image = $("#image");
    $video = $("#video");
    $audio = $("#audio");

    onEntriesUpdate();

    if(Entries.hasMedia()) {
      document.body.classList.add("has-gallery");

      initEvents();
      initLayout();
    }
  }

  return {
    init: init,
    onEntriesUpdate: onEntriesUpdate,
    go: go,
    prev: prev,
    next: next,
    rewind: rewind,
    fastForward: fastForward,
    jump: jump
  };
})();
