(() => {
  const form = document.getElementById("booking-calendar-form");
  if (!form) {
    return;
  }

  const instructorId = form.dataset.instructorId;
  const excludeId = form.dataset.excludeId || "";
  const grid = document.getElementById("calendar-grid");
  const slotsContainer = document.getElementById("slots-container");
  const label = document.getElementById("cal-label");
  const startAtInput = document.getElementById("StartAt");
  const durationSelect = document.getElementById("LessonDurationId");
  const participantsInput = document.getElementById("Participants");
  const lessonTypeSelect = document.getElementById("LessonType");
  const selectedSlotLabel = document.getElementById("selected-slot-label");
  const participantsField = document.getElementById("participants-field");

  let year = Number(form.dataset.year);
  let month = Number(form.dataset.month);
  let selectedDate = form.dataset.selectedDate || "";
  let availableDates = new Set();

  const monthNames = [
    "gennaio", "febbraio", "marzo", "aprile", "maggio", "giugno",
    "luglio", "agosto", "settembre", "ottobre", "novembre", "dicembre"
  ];

  function participants() {
    const lessonType = lessonTypeSelect ? lessonTypeSelect.value : "0";
    if (lessonType === "0") {
      return 1;
    }
    const value = Number(participantsInput?.value || 1);
    return Number.isFinite(value) && value > 0 ? value : 1;
  }

  function toggleParticipants() {
    if (!participantsField || !lessonTypeSelect) {
      return;
    }
    const isPrivate = lessonTypeSelect.value === "0";
    participantsField.classList.toggle("hidden", isPrivate);
    if (isPrivate && participantsInput) {
      participantsInput.value = "1";
    }
  }

  async function loadAvailableDays() {
    if (!durationSelect) {
      return;
    }

    const params = new URLSearchParams({
      instructorId,
      year: String(year),
      month: String(month),
      lessonDurationId: durationSelect.value,
      participants: String(participants())
    });
    if (excludeId) {
      params.set("excludeBookingId", excludeId);
    }

    const response = await fetch(`/Bookings/AvailableDays?${params.toString()}`, {
      headers: { Accept: "application/json" }
    });
    if (!response.ok) {
      availableDates = new Set();
      renderCalendar();
      return;
    }

    const data = await response.json();
    availableDates = new Set((data || []).map((d) => d.date));
    renderCalendar();

    if (selectedDate && availableDates.has(selectedDate)) {
      await loadSlots(selectedDate);
    } else if (selectedDate) {
      selectedDate = "";
      if (startAtInput) {
        startAtInput.value = "";
      }
      showSlotsHint("Seleziona un giorno nel calendario.");
      if (selectedSlotLabel) {
        selectedSlotLabel.textContent = "";
      }
    }
  }

  function renderCalendar() {
    if (!grid || !label) {
      return;
    }

    label.textContent = `${monthNames[month - 1]} ${year}`;
    grid.innerHTML = "";

    const first = new Date(year, month - 1, 1);
    // Lunedì = 0 … Domenica = 6
    const offset = (first.getDay() + 6) % 7;
    const daysInMonth = new Date(year, month, 0).getDate();

    for (let i = 0; i < offset; i += 1) {
      const empty = document.createElement("div");
      empty.className = "h-10";
      grid.appendChild(empty);
    }

    for (let day = 1; day <= daysInMonth; day += 1) {
      const dateStr = `${year}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
      const enabled = availableDates.has(dateStr);
      const btn = document.createElement("button");
      btn.type = "button";
      btn.textContent = String(day);
      btn.dataset.date = dateStr;
      btn.className = [
        "h-10 rounded text-sm font-medium transition",
        enabled
          ? "bg-primary/10 text-primary hover:bg-primary hover:text-white focus:outline-none focus:ring-2 focus:ring-primary"
          : "cursor-not-allowed bg-asphalt/5 text-asphalt-muted/50 dark:bg-asphalt-muted/20",
        selectedDate === dateStr ? "ring-2 ring-accent bg-primary text-white" : ""
      ].join(" ");
      btn.disabled = !enabled;
      if (enabled) {
        btn.addEventListener("click", () => onDayClick(dateStr));
      }
      grid.appendChild(btn);
    }
  }

  async function onDayClick(dateStr) {
    selectedDate = dateStr;
    renderCalendar();
    await loadSlots(dateStr);
  }

  function showSlotsHint(text) {
    if (!slotsContainer) {
      return;
    }
    slotsContainer.innerHTML = `<p class="text-sm text-asphalt-muted dark:text-park-concrete/70">${text}</p>`;
  }

  async function loadSlots(dateStr) {
    if (!durationSelect || !slotsContainer) {
      return;
    }

    showSlotsHint("Caricamento orari…");

    const params = new URLSearchParams({
      instructorId,
      date: dateStr,
      lessonDurationId: durationSelect.value,
      participants: String(participants())
    });
    if (excludeId) {
      params.set("excludeBookingId", excludeId);
    }

    const response = await fetch(`/Bookings/AvailableSlots?${params.toString()}`, {
      headers: { Accept: "application/json" }
    });

    if (!response.ok) {
      showSlotsHint("Impossibile caricare gli orari.");
      return;
    }

    const slots = await response.json();
    if (!slots.length) {
      showSlotsHint("Nessun orario libero per questo giorno.");
      return;
    }

    slotsContainer.innerHTML = "";
    const currentStart = startAtInput?.value || "";

    slots.forEach((slot) => {
      const btn = document.createElement("button");
      btn.type = "button";
      btn.className = [
        "btn-outline btn-sm",
        currentStart === slot.startAt ? "ring-2 ring-accent bg-primary text-white border-primary" : ""
      ].join(" ");
      btn.textContent = `${slot.label} · ${slot.remainingCapacity} posti`;
      btn.addEventListener("click", () => selectSlot(slot));
      slotsContainer.appendChild(btn);
    });
  }

  function selectSlot(slot) {
    if (startAtInput) {
      startAtInput.value = slot.startAt;
    }
    if (selectedSlotLabel) {
      selectedSlotLabel.textContent = `Orario selezionato: ${slot.startAt.replace("T", " ")}`;
    }
    loadSlots(selectedDate);
  }

  document.getElementById("cal-prev")?.addEventListener("click", () => {
    month -= 1;
    if (month < 1) {
      month = 12;
      year -= 1;
    }
    loadAvailableDays();
  });

  document.getElementById("cal-next")?.addEventListener("click", () => {
    month += 1;
    if (month > 12) {
      month = 1;
      year += 1;
    }
    loadAvailableDays();
  });

  durationSelect?.addEventListener("change", () => {
    if (startAtInput) {
      startAtInput.value = "";
    }
    if (selectedSlotLabel) {
      selectedSlotLabel.textContent = "";
    }
    loadAvailableDays();
  });

  participantsInput?.addEventListener("change", () => {
    if (startAtInput) {
      startAtInput.value = "";
    }
    loadAvailableDays();
  });

  lessonTypeSelect?.addEventListener("change", () => {
    toggleParticipants();
    if (startAtInput) {
      startAtInput.value = "";
    }
    loadAvailableDays();
  });

  form.addEventListener("submit", (event) => {
    if (!startAtInput?.value) {
      event.preventDefault();
      showSlotsHint("Seleziona un giorno e un orario libero prima di confermare.");
    }
  });

  toggleParticipants();
  loadAvailableDays().then(() => {
    if (selectedDate && startAtInput?.value) {
      if (selectedSlotLabel) {
        selectedSlotLabel.textContent = `Orario selezionato: ${startAtInput.value.replace("T", " ")}`;
      }
    }
  });
})();
