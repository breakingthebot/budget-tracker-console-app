# Changelog

## [0.6.0] - 2026-07-01
- Add GitHub Actions CI for pushes and pull requests.
- Build the console app and run the MSTest suite in the CI workflow.
- Document the automated CI workflow in the project README.

## [0.5.0] - 2026-07-01
- Add CSV import support for historical transactions.
- Add duplicate skipping when imported rows already exist in tracked data.
- Add automated tests for CSV import parsing and service-level import behavior.

## [0.4.0] - 2026-07-01
- Add CSV file export support under the local exports folder.
- Add overwrite confirmation for existing monthly export files.
- Add automated tests for CSV file writing and monthly file export behavior.

## [0.3.0] - 2026-07-01
- Add config-driven monthly category targets from JSON.
- Add over-budget reporting to the monthly report output.
- Add automated tests for target loading and over-budget status calculation.

## [0.2.0] - 2026-07-01
- Add JSON persistence so tracked entries survive between app runs.
- Add storage-path status output in the console workflow.
- Add tests for JSON file storage and service startup loading.

## [0.1.0] - 2026-07-01
- Add the initial .NET 8 budget tracker console app foundation.
- Add in-memory expense tracking, monthly reporting, and CSV export.
- Add MSTest coverage for the core budgeting service.
