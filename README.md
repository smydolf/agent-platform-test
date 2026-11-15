# agent-platform-test

A small fake app to test the agent platform (smydolf/agent-platform#40). It is not a real service.

Orders.Api stores orders in Azure SQL, keeps invoices as PDF files in Azure Blob Storage,
and gets exchange rates from a third-party HTTP API.

## Run it locally

1. Start SQL Server and set `ConnectionStrings__Orders`.
2. `dotnet run --project tools/DbSetup` makes the table and three seed orders (order 3 has no currency).
3. `dotnet run --project src/Orders.Api`. `GET /health` answers when the app runs.
