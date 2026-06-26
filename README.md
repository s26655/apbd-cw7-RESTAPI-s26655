# Mini Helpdesk

A simple ASP.NET Core MVC application for managing support tickets.

The application allows the user to:

* view the ticket list,
* create a ticket with the first comment,
* view ticket details with comments,
* close a ticket.

Creating a ticket and saving the first comment are executed in one database transaction.

## Technologies

* ASP.NET Core MVC
* Razor Views
* SQL Server
* ADO.NET with `Microsoft.Data.SqlClient`
* xUnit

## How to run the database

The application uses SQL Server. For local development, SQL Server can be started in Docker:

```bash
docker run \
  --name mini-helpdesk-sql \
  -e 'ACCEPT_EULA=Y' \
  -e 'MSSQL_SA_PASSWORD=YourStrong!Passw0rd' \
  -p 1433:1433 \
  -v mini_helpdesk_sql_data:/var/opt/mssql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

If the container already exists but is stopped, start it with:

```bash
docker start mini-helpdesk-sql
```

Create the database and tables:

```bash
docker exec -i mini-helpdesk-sql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P 'YourStrong!Passw0rd' \
  -C \
  -i /dev/stdin < MiniHelpdesk/Sql/01_create_mini_helpdesk_database.sql
```

The database name is:

```text
MiniHelpdeskDb
```

The application connection string is located in:

```text
MiniHelpdesk/appsettings.json
```

## How to run the application

From the repository root, run:

```bash
dotnet run --project MiniHelpdesk/MiniHelpdesk.csproj
```

Alternatively, to run it on a specific local HTTP address:

```bash
dotnet run --project MiniHelpdesk/MiniHelpdesk.csproj --urls "http://localhost:5257"
```

Then open:

```text
http://localhost:5257
```

or:

```text
http://localhost:5257/Tickets
```

## How to run the tests

From the repository root, run:

```bash
dotnet test MiniHelpdesk.slnx
```

## Database used

The application uses SQL Server with two tables:

* `Tickets`
* `TicketComments`

The SQL scripts are located in:

```text
MiniHelpdesk/Sql/
```

Main create script:

```text
MiniHelpdesk/Sql/01_create_mini_helpdesk_database.sql
```

Drop script:

```text
MiniHelpdesk/Sql/02_drop_mini_helpdesk_database.sql
```

## Where the middleware is

The custom middleware classes are located in:

```text
MiniHelpdesk/Middleware/
```

Files:

```text
MiniHelpdesk/Middleware/RequestTimingMiddleware.cs
MiniHelpdesk/Middleware/ExceptionHandlingMiddleware.cs
```

`RequestTimingMiddleware` logs:

* HTTP method,
* request path,
* request duration.

The middleware is registered in:

```text
MiniHelpdesk/Program.cs
```

## Where the transaction is

The transaction is implemented in:

```text
MiniHelpdesk/Repositories/TicketRepository.cs
```

Method:

```text
CreateWithFirstCommentAsync
```

This method saves:

* the new ticket,
* the first ticket comment.

Both operations use the same `SqlConnection` and `SqlTransaction`. If saving the comment fails, the transaction is rolled back and the ticket is not kept in the database.

## Where the tests are

The unit tests are located in:

```text
MiniHelpdesk.Tests/TicketServiceTests.cs
```

The tests cover:

* creating a valid ticket with the first comment,
* rejecting an empty title,
* rejecting an empty first comment,
* closing a ticket.

## Lecture questions

### Why does middleware order in Program.cs matter?

Middleware is executed in the order in which it is registered. A middleware registered earlier runs earlier for the incoming request and later for the outgoing response. This matters especially for exception handling, because it can only catch exceptions from middleware and endpoints registered after it.

### What is the difference between app.Use and app.Run?

`app.Use` can execute logic and then pass the request to the next middleware by calling `next()`. `app.Run` terminates the pipeline and does not pass the request further.

### Why should the controller not contain all application logic?

The controller should handle HTTP concerns: receiving requests, validating model state, choosing views, redirects, and status codes. Business rules belong in the service layer, and database access belongs in the repository layer. This keeps the application easier to test and maintain.

### What does a unit test of the Service layer give us?

A unit test of the Service layer verifies business rules without requiring a real database or web server. It confirms that validation and application logic work correctly in isolation.

### What should happen if saving the ticket succeeds, but saving the comment fails?

Neither the ticket nor the comment should remain in the database. Both inserts must be part of one transaction, and the transaction must be rolled back if any part fails.
