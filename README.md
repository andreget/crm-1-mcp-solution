# Crm

En liten webapp for kundebehandling (CRM). Dette er løsningsforslaget til databasemodulen (Modul 2). Kunder og aktiviteter kan nå lagres i en SQL Server-database, enten med Dapper eller med Entity Framework Core (EF), i tillegg til JSON, XML og minne. Du bytter lagring ved å endre én linje i `Program.cs`.

## Hva den gjør

- **Kunder:** liste med søk og statusfilter, detaljside, opprett, endre og slett.
- **Aktiviteter:** registrer møter, samtaler, e-poster og oppgaver for en kunde, merk dem som ferdige, og se alle åpne aktiviteter i én oversikt. Forfalte aktiviteter er markert.
- **API (JSON):** alle endepunktene under `/api`, som beskrevet under.
- **Dependency injection (DI):** `ICustomerService` og `IActivityService` er registrert, og alle endepunktene får dem som parameter.
- **Database:** tabellene `Customers` og `Activities` i LocalDB, laget med en EF-migrering. EF er registrert som standard.

## Krav

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio (en nyere versjon, som støtter `.http`-filer), eller en annen editor sammen med kommandolinjeverktøyet `dotnet`
- SQL Server LocalDB, som installeres sammen med Visual Studio (arbeidsmengden **ASP.NET and web development** eller **Data storage and processing**)
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)

Sjekk at installasjonen fungerer:

```
dotnet --version
sqllocaldb info
```

`sqllocaldb info` skal vise `MSSQLLocalDB`.

## Lag din egen kopi

1. Klikk **Use this template** øverst på denne siden, og deretter **Create a new repository**.
2. Klone det nye repoet ditt:

```
git clone <url-to-your-repository>
```

## Kjør appen

**Visual Studio:** åpne `Crm.slnx` (eller `Crm.csproj`) og trykk `F5`. Nettleseren åpner <http://localhost:5080>.

**Kommandolinjen:** i mappen som inneholder `Crm.csproj`:

```
dotnet restore
dotnet run
```

Åpne deretter <http://localhost:5080>.

> **Start alltid appen fra prosjektmappen.** Maler og datafiler leses med relative stier, så sidene feiler hvis appen startes fra en annen mappe.

## API-et

Endepunktene under `/api` returnerer data som JSON, ikke HTML.

### Kunder

| Endepunkt | Beskrivelse | Svar |
|---|---|---|
| `GET /api/customers?search=&status=` | Alle kunder, eventuelt filtrert på tekst og status | 200 |
| `GET /api/customers/{id}` | Én kunde | 200, 404 |
| `POST /api/customers` | Opprett kunde | 201, 400 |
| `PUT /api/customers/{id}` | Oppdater kunde | 200, 404, 400 |
| `DELETE /api/customers/{id}` | Slett kunde og kundens aktiviteter | 204, 404 |

### Aktiviteter

| Endepunkt | Beskrivelse | Svar |
|---|---|---|
| `GET /api/customers/{id}/activities` | Aktivitetene til en kunde, nyeste først | 200, 404 |
| `POST /api/customers/{id}/activities` | Opprett aktivitet for en kunde | 201, 404, 400 |
| `GET /api/activities?open=true` | Alle aktiviteter, eller bare åpne med `open=true` | 200 |
| `PUT /api/activities/{id}/done` | Merk som ferdig eller åpen, med `{ "done": true }` | 204, 404 |
| `DELETE /api/activities/{id}` | Slett aktivitet | 204, 404 |

### Oppslagslister

| Endepunkt | Beskrivelse | Svar |
|---|---|---|
| `GET /api/statuses` | Gyldige kundestatuser | 200 |
| `GET /api/activity-types` | Gyldige aktivitetstyper | 200 |

### Valg i løsningen

- **Validering av kunde:** `name` kan ikke være tom, `email` må inneholde `@`, og `status` må være `Lead`, `Active` eller `Inactive`.
- **Validering av aktivitet:** `title` kan ikke være tom, og `type` må være `Meeting`, `Call`, `Email` eller `Task`. Mangler `date`, brukes nåværende tidspunkt.
- **Feilsvar:** Alle 400-svar har formatet `{ "error": "melding" }`.
- **201 Created:** Svaret fra `POST /api/customers` har en `Location`-header med adressen til den nye kunden. Svaret fra `POST /api/customers/{id}/activities` har ingen, siden API-et ikke har et endepunkt for å hente én aktivitet.
- **Sletting av kunde:** Aktivitetene til kunden slettes sammen med kunden.
- **Sortering:** `GET /api/customers` sorterer på navn.
- **JSON-navn:** Egenskapene i JSON starter med liten bokstav (`name`, `email`).

### Test med `.http`-filer

- `Crm.http` inneholder de tre første kallene fra API-videoen.
- `Assignment.http` inneholder alle kallene som tester løsningen, med forventet statuskode i kommentaren over hvert kall.

Åpne filen i Visual Studio, start appen, og trykk **Send request** over et kall. Kjør kallene ovenfra og ned. Noen kall bruker variabler øverst i filen (`@newCustomerId`, `@activityId` og `@tempCustomerId`). Sett dem til id-en i svaret fra opprett-kallet før du kjører de neste. Hvis appen kjører på en annen port enn 5080, endrer du `@host`.

Alle kallene skal gi de samme statuskodene og svarene uansett hvilken implementasjon som er registrert.

## Databasen i dette repoet

### Endret siden `crm-1-db-start`

- `Migrations/` inneholder migreringen `InitialCreate`, som lager tabellene `Customers` og `Activities`.
- Nye implementasjoner: `EfCustomerService`, `EfActivityService`, `DapperCustomerService` og `DapperActivityService`.
- Pakken `Dapper` står i `Crm.csproj`.
- `Program.cs` registrerer EF-versjonene, og fyller tabellen `Customers` fra `customers.json` første gang appen starter mot en tom database.

### Fem implementasjoner bak hvert interface

| Interface | Implementasjon | Lagring | Registrering |
|---|---|---|---|
| `ICustomerService` | `EfCustomerService` | Databasen, via EF | `AddScoped` |
| | `DapperCustomerService` | Databasen, via Dapper og egen SQL | `AddScoped` |
| `ICustomerService` | `JsonCustomerService` | `customers.json` | `AddScoped` |
| | `XmlCustomerService` | `customers.xml` | `AddScoped` |
| | `InMemoryCustomerService` | Liste i minnet, med de fem eksempelkundene | `AddSingleton` |
| `IActivityService` | `EfActivityService` | Databasen, via EF | `AddScoped` |
| | `DapperActivityService` | Databasen, via Dapper og egen SQL | `AddScoped` |
| | `JsonActivityService` | `activities.json` | `AddScoped` |
| | `XmlActivityService` | `activities.xml` | `AddScoped` |
| | `InMemoryActivityService` | Liste i minnet, starter tom | `AddSingleton` |

### Dapper og EF

Begge snakker med den samme databasen, men på ulike måter:

- **Dapper:** du skriver SQL-en selv, og Dapper gjør radene om til `Customer`- og `Activity`-objekter. Du ser nøyaktig hvilke spørringer som kjøres.
- **EF:** du skriver LINQ mot `CrmDbContext`, og EF lager SQL-en. Den genererte SQL-en vises i konsollen, fordi `appsettings.json` logger `Microsoft.EntityFrameworkCore.Database.Command`.

Tabellene lages av EF-migreringen i begge tilfeller. Dapper bruker dem bare.

### Kom i gang med databasen

Kjør dette i Package Manager Console:

```
Update-Database
```

Start appen. Tabellen `Customers` fylles med de fem eksempelkundene, med id 1–5, første gang. Kjør deretter `Assignment.http`.

### Bytt implementasjon

Øverst i `Program.cs` står én aktiv linje per interface. Flytt kommentaren `//` for å bytte:

```csharp
builder.Services.AddScoped<ICustomerService, EfCustomerService>();
//builder.Services.AddScoped<ICustomerService, DapperCustomerService>();
//builder.Services.AddScoped<ICustomerService, JsonCustomerService>();
```

Når aktivitetene ligger i databasen, må kundene også gjøre det. Fremmednøkkelen krever at kunden finnes i tabellen `Customers`. EF og Dapper kan godt blandes, siden begge bruker de samme tabellene.

### Koble til med SSMS

- **Server name:** `(localdb)\MSSQLLocalDB`
- **Authentication:** Windows Authentication

## Prosjektstruktur

```
Program.cs          Endepunkter, hjelpefunksjoner, registrering av tjenester (DI) og seeding
appsettings.json    Tilkoblingsstrengen til databasen og logging av SQL fra EF
Data/               CrmDbContext: tabellene og koblingen mellom dem
Migrations/         EF-migreringen som lager tabellene
Models/             Customer og Activity
Services/           ICustomerService og IActivityService, med EF-, Dapper-, JSON-, XML- og minneimplementasjoner
Templates/          HTML-maler med {{placeholders}}
wwwroot/            CSS og JavaScript
customers.json      Eksempelkunder i JSON-format, også brukt til å fylle databasen
customers.xml       Eksempelkunder i XML-format (appen endrer denne filen)
Crm.http            De tre første kallene fra API-videoen
Assignment.http     Alle kallene som tester API-et. Skal gi samme svar med alle implementasjonene
Log.md              Logg og designnotater
TestResults.md      Resultatene fra kallene i Assignment.http
```

## Data

- Databasen ligger i LocalDB, utenfor prosjektmappen, og følges ikke av Git. Slik nullstiller du den. Neste oppstart fyller inn de fem kundene igjen:

```
Drop-Database
Update-Database
```

- `customers.json` og `customers.xml` inneholder fem eksempelkunder. Appen skriver til filen som hører til den registrerte implementasjonen. Gjenopprett med `git checkout customers.json` og `git checkout customers.xml`.
- `activities.json` og `activities.xml` opprettes første gang du legger til en aktivitet med JSON- eller XML-versjonen. De følges ikke av Git.
- Versjonene i minnet nullstilles hver gang appen startes på nytt.

Nullstill dataene før du kjører hele `Assignment.http` på nytt. Standardverdiene for variablene øverst i filen stemmer bare med ferske data, med kunde 1–5.

## Feilsøking

| Problem | Hva du bør sjekke |
|---|---|
| Feilen `NETSDK1004` (assets file not found) | Kjør `dotnet restore` i mappen med `Crm.csproj`, og bygg på nytt. |
| Feil eller blank side rett etter oppstart | Appen er startet fra feil mappe. Start den fra mappen med `Crm.csproj`. |
| Port 5080 er opptatt | Endre `applicationUrl` i `Properties/launchSettings.json`, og `@host` i `.http`-filene. |
| NuGet prøver å installere versjon 11 av EF-pakkene | Prosjektet bruker .NET 10. Velg nyeste 10.x, eller bruk `Version="10.*"` i `Crm.csproj`. |
| `Add-Migration` finner ikke `CrmDbContext` | `AddDbContext` mangler i `Program.cs`, eller prosjektet bygger ikke. Bygg og prøv igjen. |
| `Cannot open database "Crm"` | Databasen finnes ikke ennå. Kjør `Update-Database`. |
| Feil om `FOREIGN KEY` når du legger til en aktivitet | Kunden finnes ikke i tabellen `Customers`. Kunder og aktiviteter må bruke samme lagring når aktivitetene ligger i databasen. |
| Appen feiler med at en tjeneste ikke kan løses | Interfacet er ikke registrert med `builder.Services` i `Program.cs`, eller ingen linje for det interfacet er aktiv. |
| En kunde eller aktivitet forsvinner rett etter at den er opprettet | Versjonen i minnet er registrert med `AddScoped`. Bruk `AddSingleton`. |
| Et kall i `Assignment.http` gir 404 for en id som skulle finnes | Variablene øverst i filen peker på feil id, eller dataene er ikke nullstilt. |