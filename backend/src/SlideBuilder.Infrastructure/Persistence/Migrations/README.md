# EF Core Migrations

To add a new migration:
```bash
dotnet ef migrations add InitialCreate --project SlideBuilder.Infrastructure --startup-project SlideBuilder.Api
```

To update the database:
```bash
dotnet ef database update --project SlideBuilder.Infrastructure --startup-project SlideBuilder.Api
```

Note: Ensure `dotnet-ef` tool is installed globally:
```bash
dotnet tool install --global dotnet-ef
```
