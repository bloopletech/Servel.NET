"use strict";

async function init() {
  document.title = "Home";

  const response = await fetch(window.location.href, {
    headers: {
      "Accept": "application/json",
    }
  });

  const roots = await response.json();

  const rows = roots.map(root => HTMLSafe`
      <li>
        <a href="${root.url}">${root.name}</a>
      </li>
    `);

  $("#container").innerHTML = `
    <ul id="roots">
      ${rows.join("")}
    </ul>
  `;
}

window.addEventListener("DOMContentLoaded", init);
