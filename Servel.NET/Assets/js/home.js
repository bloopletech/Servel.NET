"use strict";

async function init() {
  document.title = "Browse Listings";

  const response = await fetch(window.location.href, {
    headers: {
      "Accept": "application/json",
    }
  });

  const listings = await response.json();

  const items = listings.map(listing => HTMLSafe`
      <li>
        <a href="${listing}">${listing}</a>
      </li>
    `);

  $("#container").innerHTML = `
    <ul id="roots">
      ${items.join("")}
    </ul>
  `;
}

window.addEventListener("DOMContentLoaded", init);
