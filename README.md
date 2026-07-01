# Budget Tracker Console App

A .NET 8 console app for tracking configurable expense categories, saving data locally, checking category targets, tracking savings progress, reviewing month-end budget health, exporting monthly data to CSV files, and importing historical transactions from CSV.

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
2. Use the menu to add expenses, view a monthly report with target warnings, export CSV files, import CSV files, or list entries.
3. Run `dotnet run --project src/BudgetTracker.App -- --version` to print the app version.
4. Re-run the app to confirm previous entries reload from `data/budget-entries.json`.

## Deployed
Not deployed. This is a local console application.

## Architecture Notes
This version adds a real month-end readout on top of the existing category target logic, including dedicated savings-goal progress instead of treating savings like a normal overspend category. I kept that behavior data-driven by extending target configuration with evaluation modes, so the report builder can distinguish between “stay under this amount” and “work toward this goal” without hard-coding one-off logic into the console flow.

## Notes
- Budget data is stored locally at `data/budget-entries.json` and that folder is ignored by Git.
- Budget categories are checked into `src/BudgetTracker.App/Configuration/categories.json`.
- Monthly category targets are checked into `src/BudgetTracker.App/Configuration/budget-targets.json`.
- Savings targets can use `"evaluationMode": "min-progress"` to behave like goals instead of overspend limits.
- CSV exports are written to `exports/budget-export-yyyy-MM.csv` and that folder is ignored by Git.
- CSV imports must use the header `Date,Category,Description,Amount`.
- GitHub Actions CI restores dependencies, builds the console app, and runs the MSTest suite on every push and pull request.
- Logging is structured to make later debugging and file-based logging easier.
