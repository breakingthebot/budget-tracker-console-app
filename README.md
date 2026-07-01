# Budget Tracker Console App

A .NET 8 console app for tracking categorized expenses, saving them locally, checking category targets, reviewing monthly spending, exporting monthly data to CSV files, and importing historical transactions from CSV.

## Stack
- C# / .NET 8
- MSTest for automated tests
- Built-in .NET libraries only for runtime logic

## Setup
1. Ensure .NET SDK 8 is installed.
2. Clone the repository.
3. From the repository root, run `dotnet restore`.
4. The app will create `data/budget-entries.json` automatically the first time you save an entry.
5. Monthly category targets are loaded from `src/BudgetTracker.App/Configuration/budget-targets.json`.
6. CSV exports are written to the local `exports` folder.

## Environment Variables
No environment variables are required right now. See `.env.example`.

## Running Locally
1. Run `dotnet run --project src/BudgetTracker.App`.
2. Use the menu to add expenses, view a monthly report with target warnings, export CSV files, import CSV files, or list entries.
3. Run `dotnet run --project src/BudgetTracker.App -- --version` to print the app version.
4. Re-run the app to confirm previous entries reload from `data/budget-entries.json`.

## Deployed
Not deployed. This is a local console application.

## Architecture Notes
This version completes the CSV workflow in both directions by letting the app import historical transactions from files as well as export them. I split parsing from the main budget service so CSV-specific validation, header checks, and row-level errors live in one place, while the tracker service stays responsible for deduplicating imported rows and deciding what gets persisted.

## Notes
- Budget data is stored locally at `data/budget-entries.json` and that folder is ignored by Git.
- Monthly category targets are checked into `src/BudgetTracker.App/Configuration/budget-targets.json`.
- CSV exports are written to `exports/budget-export-yyyy-MM.csv` and that folder is ignored by Git.
- CSV imports must use the header `Date,Category,Description,Amount`.
- Logging is structured to make later debugging and file-based logging easier.
