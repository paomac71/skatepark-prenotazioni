# Skate Park Prenotazioni

Applicazione ASP.NET Core MVC (.NET 8) per prenotare lezioni di skate (private e di gruppo).

## Stack

- ASP.NET Core MVC + Identity (email/password)
- Login Google OAuth (stub da configurazione; attivato solo se ClientId/Secret reali)
- Entity Framework Core + SQL Server
- Open-Meteo per previsioni settimanali (lat/lon in `appsettings.json`)
- Tailwind CSS per la UI (brand outdoor skate park)

## Avvio rapido

Prerequisiti: .NET 8 SDK, SQL Server o LocalDB. Node.js solo se ricostruisci il CSS.

```bash
cd SkateParkPrenotazioni
dotnet restore
dotnet ef database update   # opzionale: la migrazione parte anche all'avvio
dotnet run
```

Apri l’URL mostrato dalla console (tipicamente `https://localhost:7xxx`).

### CSS (Tailwind)

Il file `wwwroot/css/site.css` è già generato. Per modificarlo:

```bash
cd SkateParkPrenotazioni
npm install
npm run build:css    # oppure: npm run watch:css
```

## Configurazione

File: `SkateParkPrenotazioni/appsettings.json`

- **ConnectionStrings:DefaultConnection** — SQL Server / LocalDB
- **Authentication:Google** — sostituisci `YOUR_GOOGLE_CLIENT_ID` / `YOUR_GOOGLE_CLIENT_SECRET` con le credenziali della Google Cloud Console (redirect URI Identity: `/signin-google`)
- **Weather** — `Latitude`, `Longitude`, `Timezone` (default Milano)

Non committare secret reali: usa User Secrets o variabili d’ambiente in produzione.

## Funzionalità MVP

- Registrazione / login utente
- Elenco istruttori con capienza massima
- Creazione, modifica e annullamento delle proprie prenotazioni
- Controllo capienza per istruttore/slot
- Pagina meteo 7 giorni (Open-Meteo)

## Test

```bash
dotnet test
```

## Seed iniziale

All’avvio vengono applicate le migrazioni e, se il DB è vuoto, creati:

- 3 istruttori (Luca, Giulia, Marco)
- 3 durate lezione (30 / 60 / 90 minuti)

## Struttura

```
SkateParkPrenotazioni.sln
.cursor/rules/           Regole progetto (architettura, MVC, Tailwind, …)
SkateParkPrenotazioni/
  Controllers/           Thin controllers (Home, Bookings, Instructors, Weather)
  Services/              Logica applicativa + Open-Meteo / capienza
  ViewModels/            Modelli tipizzati per le view
  Views/                 UI in italiano (Tailwind)
  Data/                  ApplicationDbContext, DbSeeder, Migrations
SkateParkPrenotazioni.Tests/
```
