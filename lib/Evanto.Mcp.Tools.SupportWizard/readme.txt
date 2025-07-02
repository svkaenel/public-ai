Anlegen Initialmigration:

dotnet tool install --global dotnet-ef bzw. dotnet tool update --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add InitialCreate
dotnet ef database update


Quelle: https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli

