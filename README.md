# Budget Tracker Console App

A .NET 8 console app for tracking categorized expenses, saving them locally, reviewing monthly spending, and exporting monthly data as CSV.

## Stack
- C# / .NET 8
- MSTest for automated tests
- Built-in .NET libraries only for runtime logic

## Setup
1. Ensure .NET SDK 8 is installed.
2. Clone the repository.
3. From the repository root, run `dotnet restore`.
4. The app will create `data/budget-entries.json` automatically the first time you save an entry.

## Environment Variables
No environment variables are required right now. See `.env.example`.

## Running Locally
1. Run `dotnet run --project src/BudgetTracker.App`.
2. Use the menu to add expenses, view a monthly report, export CSV, or list entries.
3. Run `dotnet run --project src/BudgetTracker.App -- --version` to print the app version.
4. Re-run the app to confirm previous entries reload from `data/budget-entries.json`.

## Deployed
Not deployed. This is a local console application.

## Architecture Notes
This version makes the tracker actually usable day to day because entries now survive between runs. I kept the console app thin and pushed the storage logic behind a dedicated interface in the core layer, so the app can load and save JSON today without coupling the reporting logic to one persistence approach forever.

## Notes
- Budget data is stored locally at `data/budget-entries.json` and that folder is ignored by Git.
- Logging is structured to make later debugging and file-based logging easier.
