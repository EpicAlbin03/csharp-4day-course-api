- adding rows in AppDbContext gets added during migration
- prefer overloading on constructors, optional params on functions
- beekeeper studio
- specify default value after changing model so it applies to existing rows after migration
- .NET migrations do not work, always check the migration is correct!
- use in-memory db until launch, then do single migration when launching
- no back-references or internal flags on DTOs
- singleton: only one instance can be created

## Postgres

- `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL`
- `dotnet ef migrations add InitialCreate` <!-- initial migration -->
- make modifications
- `dotnet ef migrations add AddAccountCurrency` <!-- create migration -->
- `dotnet ef database update` <!-- apply changes -->
- `dotnet ef database update InitialCreate` <!-- rollback to InitialCreate migration -->
- undo changes
- `dotnet ef migrations remove` <!-- delete latest migration & updates snapshot -->
