"use strict";

var Gallery = (function() {
  var LAYOUT_MODES = ["fit-both", "fit-width", "clamp-width", "full-size"];

  var $body;
  var $gallery;
  var $image;
  var $video;
  var $audio;
  var currentIndex;
  var layoutModeIndex = 0;

  function renderText(url) {
    var http = new XMLHttpRequest();
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

    var entry = Entries.media()[currentIndex];

    var url = entry.href;
    var type = entry.mediaType;

    $gallery.classList.add(type);

    var $element;
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
    var newIndex = clamp(index);
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
    var index = Entries.media().findIndex(function(entry) {
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

      if(e.keyCode == 39 || ((e.keyCode == 32 || e.keyCode == 13) && atBottom())) {
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
    var vw = document.documentElement.clientWidth;
    var vh = document.documentElement.clientHeight;

    var viewportOrientation = vw > vh ? "landscape" : "portrait";
    $body.classList.remove("landscape", "portrait");
    $body.classList.add(viewportOrientation);

    $gallery.style.height = vh + "px";

    var scrollerMaxHeight = viewportOrientation == "landscape" ? vh : vh - 75;

    var layoutMode = LAYOUT_MODES[layoutModeIndex];

    if(layoutMode == "fit-both") {
      var maxWidth = "100%";
      var maxHeight = (scrollerMaxHeight + "px");
    }
    else if(layoutMode == "fit-width") {
      var maxWidth = "100%";
      var maxHeight = "none";
    }
    else if(layoutMode == "clamp-width") {
      var maxWidth = "1000px";
      var maxHeight = "none";
    }
    else if(layoutMode == "full-size") {
      var maxWidth = "none";
      var maxHeight = "none";
    }

    $image.style.maxWidth = maxWidth;
    $image.style.maxHeight = maxHeight;
    $video.style.maxWidth = maxWidth;
    $video.style.maxHeight = maxHeight;
    $audio.style.maxWidth = maxWidth;
    $audio.style.maxHeight = maxHeight;
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

window.addEventListener("DOMContentLoaded", Gallery.init);