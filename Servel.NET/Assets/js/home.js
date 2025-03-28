"use strict";

async function init() {
  document.title = "Browse Listings";

  const response = await fetch(window.location.href, {
    headers: {
      "Accept": "application/json",
    }
  });

  const entries = await response.json();

  const items = entries.map(entry => HTMLSafe`
      <li>
        <a href="${entry.url}">${entry.name}</a>
      </li>
    `);

  $("#container").innerHTML = `
    <ul id="listings">
      ${items.join("")}
    </ul>
  `;
}

window.addEventListener("DOMContentLoaded", init);
