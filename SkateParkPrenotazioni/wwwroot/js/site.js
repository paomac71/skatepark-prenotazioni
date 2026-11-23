(() => {
  const toggle = document.getElementById("nav-toggle");
  const menu = document.getElementById("nav-menu");
  if (!toggle || !menu) {
    return;
  }

  toggle.addEventListener("click", () => {
    const willOpen = menu.classList.contains("hidden");
    menu.classList.toggle("hidden", !willOpen);
    menu.classList.toggle("flex", willOpen);
    toggle.setAttribute("aria-expanded", willOpen ? "true" : "false");
  });
})();
