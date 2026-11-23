(() => {
  const container = document.getElementById("availability-rows");
  const template = document.getElementById("availability-row-template");
  const addButton = document.getElementById("add-availability");

  if (!container || !template || !addButton) {
    return;
  }

  function reindex() {
    const rows = container.querySelectorAll(".availability-row");
    rows.forEach((row, index) => {
      row.querySelectorAll("[name], [data-name]").forEach((el) => {
        const field = el.getAttribute("data-name") || extractField(el.getAttribute("name"));
        if (!field) {
          return;
        }
        el.setAttribute("data-name", field);
        el.setAttribute("name", `Availabilities[${index}].${field}`);
      });
    });
  }

  function extractField(name) {
    if (!name) {
      return null;
    }
    const match = name.match(/Availabilities\[\d+\]\.(.+)$/);
    return match ? match[1] : null;
  }

  addButton.addEventListener("click", () => {
    const fragment = template.content.cloneNode(true);
    container.appendChild(fragment);
    reindex();
  });

  container.addEventListener("click", (event) => {
    const target = event.target;
    if (!(target instanceof HTMLElement)) {
      return;
    }
    if (target.classList.contains("remove-availability")) {
      const row = target.closest(".availability-row");
      row?.remove();
      reindex();
    }
  });

  reindex();
})();
