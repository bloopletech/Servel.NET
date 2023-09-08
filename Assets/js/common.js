"use strict";

function atBottom() {
  return (window.scrollY + window.innerHeight) == document.body.scrollHeight;
}

function common() {
  window.$ = document.querySelector.bind(document);
}

window.addEventListener("DOMContentLoaded", common);