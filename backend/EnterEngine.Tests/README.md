# Test workspace

xUnit is installed and the project references EnterEngine.Api. No tests are implemented yet.

Add test classes here as you implement behaviour. Prioritise category boundaries, gaps,
overlaps and vehicle validation. Use SQL Server integration tests later for persistence,
atomic category updates and reclassification of existing vehicles.

Run from the repository root:

```sh
dotnet test backend/EnterEngine.slnx
```

A successful command with zero discovered tests is not evidence of tested behaviour.
