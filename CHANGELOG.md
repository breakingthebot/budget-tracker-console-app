# Changelog

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
