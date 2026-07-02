# Budget Tracker Console App

A .NET 8 console app for tracking configurable expense categories, saving data locally, checking category targets, editing monthly budget targets, auditing and rolling back target-change history, tracking savings progress, reviewing month-end budget health, exporting monthly data and monthly report summaries to CSV files, and previewing CSV imports before applying them.

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
2. Use the menu to add expenses, view a monthly report with target warnings, export transaction CSV files, export monthly report CSV files, import CSV files, list entries, edit monthly category targets, review target-change history, or roll a target back from history.
3. Run `dotnet run --project src/BudgetTracker.App -- --version` to print the app version.
4. Re-run the app to confirm previous entries reload from `data/budget-entries.json`.

## Deployed
Not deployed. This is a local console application.

## Architecture Notes
This version adds a second export workflow for monthly report summaries so the app can produce shareable reporting artifacts instead of only raw transaction dumps. The export stays modular: transaction rows still use the original entry exporter, while report summaries use a separate report-oriented CSV builder that writes summary metrics, savings progress, category breakdowns, and budget-status rows into a predictable file.

## Notes
- Budget data is stored locally at `data/budget-entries.json` and that folder is ignored by Git.
- Budget categories are checked into `src/BudgetTracker.App/Configuration/categories.json`.
- Monthly category targets are checked into `src/BudgetTracker.App/Configuration/budget-targets.json`.
- Monthly target edits update `src/BudgetTracker.App/Configuration/budget-targets.json` while preserving each target's evaluation mode.
- Budget target audit history is stored locally at `data/budget-target-history.json` and that file is ignored by Git.
- Target rollback records a fresh audit entry, so history stays append-only instead of silently rewriting prior changes.
- Savings targets can use `"evaluationMode": "min-progress"` to behave like goals instead of overspend limits.
- CSV exports are written to `exports/budget-export-yyyy-MM.csv` and that folder is ignored by Git.
- Monthly report CSV exports are written to `exports/budget-report-yyyy-MM.csv` and that folder is ignored by Git.
- CSV imports must use the header `Date,Category,Description,Amount`.
- CSV imports preview new rows and duplicates before anything is saved.
- Target history can be filtered to one configured category from the console menu.
- Target rollback only works when the current target still matches the selected historical value, which prevents stale rollbacks.
- GitHub Actions CI restores dependencies, builds the console app, and runs the MSTest suite on every push and pull request.
- Logging is structured to make later debugging and file-based logging easier.
