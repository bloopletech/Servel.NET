"use strict";

var Common = (function () {
  var dateFormatter = new Intl.DateTimeFormat('en-AU', {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: 'numeric', minute: 'numeric',
    hour12: true
  });

  function formatDate(date) {
    return dateFormatter.format(date).replace(",", "").replace(/am$/, "AM").replace(/pm$/, "PM");
  }

  function formatTimestamp(timestamp) {
    return formatDate(Date.parse(timestamp));
  }

  function format(input) {
    var type = typeof input;
    if(input instanceof Date) return formatDate(input);
    if(type == "number") return input.toLocaleString();
    return input;
  }

  function escapeHTML(unsafe) {
    if(unsafe == null) return "";
    return unsafe.toString()
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  function formatThenEscape(input) {
    return escapeHTML(format(input));
  }

  function atBottom() {
    return (window.scrollY + window.innerHeight) == document.body.scrollHeight;
  }

  return {
    formatDate: formatDate,
    formatTimestamp: formatTimestamp,
    format: format,
    escapeHTML: escapeHTML,
    formatThenEscape: formatThenEscape,
    atBottom: atBottom
  }
})();

window.e = Common.escapeHTML;
window.f = Common.format;
window.ef = Common.formatThenEscape;

window.addEventListener("DOMContentLoaded", () => window.$ = document.querySelector.bind(document));