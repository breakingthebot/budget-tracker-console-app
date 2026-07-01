# Budget Tracker Console App

A .NET 8 console app for tracking categorized expenses, reviewing monthly spending, and exporting monthly data as CSV.

## Stack
- C# / .NET 8
- MSTest for automated tests
- Built-in .NET libraries only for runtime logic

## Setup
1. Ensure .NET SDK 8 is installed.
2. Clone the repository.
3. From the repository root, run `dotnet restore`.

## Environment Variables
No environment variables are required right now. See `.env.example`.

## Running Locally
1. Run `dotnet run --project src/BudgetTracker.App`.
2. Use the menu to add expenses, view a monthly report, export CSV, or list entries.
3. Run `dotnet run --project src/BudgetTracker.App -- --version` to print the app version.

## Deployed
Not deployed. This is a local console application.

## Architecture Notes
This first iteration is the core budgeting loop: you can enter expenses with a category, keep everything in memory for the current session, ask for a monthly summary, and dump a month to CSV. I split the app into a thin console project and a separate core library so the input/output code stays isolated from the real business logic, which makes the reporting and export code easier to test and easier to swap later when persistence gets added.

## Notes
- Data is session-only in iteration 1 and is not persisted to disk yet.
- Logging is structured to make later debugging and file-based logging easier.
