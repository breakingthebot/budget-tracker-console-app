# Budget Tracker Console App

A .NET 8 console app for tracking configurable expense categories, saving data locally, checking category targets, tracking savings progress, reviewing month-end budget health, exporting monthly data to CSV files, previewing CSV imports before applying them, and now opening with a complete solution structure for app, core, and tests.

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
This version cleans up the repository’s solution structure so the `.sln` actually matches the real codebase instead of only pointing at tests. That sounds small, but it matters because solution-level restore, build, and test commands are now much more intuitive for anyone opening the repo in Visual Studio, VS Code, or CI tooling.

## Notes
- Budget data is stored locally at `data/budget-entries.json` and that folder is ignored by Git.
- Budget categories are checked into `src/BudgetTracker.App/Configuration/categories.json`.
- Monthly category targets are checked into `src/BudgetTracker.App/Configuration/budget-targets.json`.
- Savings targets can use `"evaluationMode": "min-progress"` to behave like goals instead of overspend limits.
- CSV exports are written to `exports/budget-export-yyyy-MM.csv` and that folder is ignored by Git.
- CSV imports must use the header `Date,Category,Description,Amount`.
- CSV imports now preview new rows and duplicates before anything is saved.
- The solution file now includes the app, core library, and test project together.
- GitHub Actions CI restores dependencies, builds the console app, and runs the MSTest suite on every push and pull request.
- Logging is structured to make later debugging and file-based logging easier.
