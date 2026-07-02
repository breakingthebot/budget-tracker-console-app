# Budget Tracker Console App

A .NET 8 console app for tracking configurable expense categories, saving data locally, checking category targets, editing monthly budget targets, auditing target-change history, tracking savings progress, reviewing month-end budget health, exporting monthly data to CSV files, and previewing CSV imports before applying them.

## Stack
- C# / .NET 8
- MSTest for automated tests
- Built-in .NET libraries only for runtime logic

## Setup
1. Ensure .NET SDK 8 is installed.
2. Clone the repository.
3. From the repository root, run `dotnet restore`.
4. The app will create `data/budget-entries.json` automatically the first time you save an entry.
5. Budget categories are loaded from `src/BudgetTracker.App/Configuration/categories.json`.
6. Monthly category targets are loaded from `src/BudgetTracker.App/Configuration/budget-targets.json`.
7. CSV exports are written to the local `exports` folder.
8. GitHub Actions runs CI for pushes and pull requests.

## Environment Variables
No environment variables are required right now. See `.env.example`.

## Running Locally
1. Run `dotnet run --project src/BudgetTracker.App`.
2. Use the menu to add expenses, view a monthly report with target warnings, export CSV files, import CSV files, list entries, edit monthly category targets, or review target-change history.
3. Run `dotnet run --project src/BudgetTracker.App -- --version` to print the app version.
4. Re-run the app to confirm previous entries reload from `data/budget-entries.json`.

## Deployed
Not deployed. This is a local console application.

## Architecture Notes
This version adds a real audit trail for category budget changes, which moves the project beyond simple config editing into a more realistic operational workflow. Target definitions still live in checked-in configuration, but every actual change now produces a separate runtime history record under `data/`, and the console can query that history directly so you can answer what changed, when it changed, and which category was affected.

## Notes
- Budget data is stored locally at `data/budget-entries.json` and that folder is ignored by Git.
- Budget categories are checked into `src/BudgetTracker.App/Configuration/categories.json`.
- Monthly category targets are checked into `src/BudgetTracker.App/Configuration/budget-targets.json`.
- Monthly target edits update `src/BudgetTracker.App/Configuration/budget-targets.json` while preserving each target's evaluation mode.
- Budget target audit history is stored locally at `data/budget-target-history.json` and that file is ignored by Git.
- Savings targets can use `"evaluationMode": "min-progress"` to behave like goals instead of overspend limits.
- CSV exports are written to `exports/budget-export-yyyy-MM.csv` and that folder is ignored by Git.
- CSV imports must use the header `Date,Category,Description,Amount`.
- CSV imports preview new rows and duplicates before anything is saved.
- Target history can be filtered to one configured category from the console menu.
- GitHub Actions CI restores dependencies, builds the console app, and runs the MSTest suite on every push and pull request.
- Logging is structured to make later debugging and file-based logging easier.
