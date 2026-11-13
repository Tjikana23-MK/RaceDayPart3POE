# RaceDay — Part 2: RESTful API Development

RaceDay is an ASP.NET Core Web API for South African road-running, walking and cycling events. This Part 2 implementation uses C#, Entity Framework Core (Code First), SQL Server, Swagger, cookie/session authentication, BCrypt password hashing, unit tests and GitHub Actions.

> **Important:** review this code, run it on your machine, and make sure you can explain every class and security decision. The POE explicitly requires you to disclose AI assistance in a footnote/comments section when used. This package is a development starter, not proof that it has been run successfully on your particular computer. Do not claim tests or CI passed until you have run them and seen the green result.

## Roles
- **Organiser:** create/update/delete their own events, manage categories, view enrolments for their events, and record/update results.
- **Participant:** browse events/categories, enrol in an event, view/withdraw own enrolments, and view own results.
- Public: registration, event browsing/details, event-type browsing.

## Project structure
```text
RaceDay_Part2/
├── RaceDay.sln
├── RaceDay.Api/                 # ASP.NET Core API
│   ├── Controllers/             # Auth, profile, events, categories, enrolments, results
│   ├── Data/RaceDayDbContext.cs  # EF Core model configuration
│   ├── DTOs/Requests.cs
│   ├── Models/Entities.cs
│   ├── Services/AuthService.cs   # BCrypt registration/login validation
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.Tests/               # xUnit authentication tests
├── .github/workflows/part2-ci.yml
└── docs/
```

## Requirements
- Visual Studio 2022 with the **ASP.NET and web development** workload, or .NET 8 SDK.
- SQL Server LocalDB / SQL Server and SQL Server Management Studio.
- Internet access for NuGet package restore.

## Configure and run in Visual Studio 2022
1. Extract this ZIP to a folder you can find, such as Documents.
2. Open `RaceDay.sln` in Visual Studio 2022.
3. In Solution Explorer, right-click the solution and choose **Restore NuGet Packages** (or build once to restore).
4. Confirm SQL Server LocalDB is installed. The default connection string in `RaceDay.Api/appsettings.json` uses `(localdb)\MSSQLLocalDB` and database `RaceDayDB`. Change it if your SQL Server instance uses another name.
5. Open **Tools → NuGet Package Manager → Package Manager Console**. Set the Default project to `RaceDay.Api`, then run:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```
   If EF tools are not installed, run `Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.11` in the Package Manager Console, then repeat. Alternatively install `dotnet-ef` and use the CLI commands below.
6. Set `RaceDay.Api` as the startup project and press **F5**. Swagger should open in your browser in Development mode. If it does not, navigate to `/swagger` on the local URL shown by Visual Studio.
7. Register an Organiser account and a Participant account through `POST /api/auth/register`. Registration accepts only `Organiser` or `Participant`; use strong test passwords.
8. In Swagger, call `POST /api/auth/login` with the same browser session. The API sets an HTTP-only authentication cookie and also records the user ID and role in ASP.NET Core Session. Use the same Swagger browser session for subsequent protected requests. Logout with `POST /api/auth/logout`.

### .NET CLI alternative
Run these commands from the `RaceDay_Part2` directory after installing the .NET 8 SDK and EF CLI:
```bash
dotnet tool install --global dotnet-ef --version 8.0.11
dotnet restore RaceDay.sln
dotnet ef migrations add InitialCreate --project RaceDay.Api --startup-project RaceDay.Api
dotnet ef database update --project RaceDay.Api --startup-project RaceDay.Api
dotnet run --project RaceDay.Api
dotnet test RaceDay.sln --configuration Release
```

## API route overview
| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register an Organiser or Participant |
| POST | `/api/auth/login` | Public | Validate credentials and create authenticated session |
| POST | `/api/auth/logout` | Authenticated | Clear session and sign out |
| GET | `/api/auth/session` | Public | Check current authentication status |
| GET / PUT | `/api/profile` | Authenticated | View/update own profile |
| GET | `/api/event-types` | Public | List Run, Walk and Cycle |
| GET | `/api/events` | Public | Browse events; optional `search` and `from` query parameters |
| GET | `/api/events/{id}` | Public | Event details and categories |
| POST | `/api/events` | Organiser | Create event |
| PUT / DELETE | `/api/events/{id}` | Owning Organiser | Update/delete own event |
| GET | `/api/events/{eventId}/categories` | Public | List event categories |
| POST | `/api/events/{eventId}/categories` | Owning Organiser | Add category |
| PUT / DELETE | `/api/events/{eventId}/categories/{categoryId}` | Owning Organiser | Update/delete category |
| POST | `/api/enrolments` | Participant | Enrol in an event with a category |
| GET | `/api/enrolments/mine` | Participant | List own enrolments and results |
| DELETE | `/api/enrolments/{id}` | Enrolment owner | Withdraw own enrolment |
| GET | `/api/enrolments/event/{eventId}` | Owning Organiser | View enrolments for own event |
| POST / PUT | `/api/results/enrolment/{enrolmentId}` | Owning Organiser | Record/update result |
| GET | `/api/results/mine` | Participant | View personal results |
| GET | `/api/results/event/{eventId}` | Owning Organiser | List results for own event |

Typical responses include `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, and `409 Conflict`. Swagger is the authoritative live endpoint explorer for the code.

## Data model
The Code-First EF Core model has six entities: `Users`, `EventTypes`, `Events`, `Categories`, `Enrolments`, and `Results`. Event types are seeded as Run, Walk and Cycle. User email and participant/event enrolment combinations are unique; an enrolment can have at most one result. Confirm that this matches the exact approved Part 1 ERD and SQL script before submission, and document any mismatch rather than silently changing the approved plan.

## Tests and CI
The `RaceDay.Tests` xUnit project contains tests for password hashing, role validation, duplicate email rejection, and login success/failure. These are service-level tests using EF Core InMemory; they do not replace HTTP integration tests for every controller/role combination. Run:
```bash
dotnet test RaceDay.sln --configuration Release
```
GitHub Actions in `.github/workflows/part2-ci.yml` restores, builds, and tests the solution on push and pull request. Push this folder to your own repository, open **Actions**, and wait for a green check. Add a screenshot of your real successful workflow run here before submitting; do not use a fabricated screenshot.

## GitHub commits and submission checklist
The assessment brief requires **at least 20 meaningful commits for Part 2**. Make commits as you complete and understand real milestones (models, DbContext, auth, profile, events, categories, enrolments, results, tests, Swagger, CI, documentation). Do not manufacture commit history in a prebuilt ZIP. Push to the GitHub repository specified by your lecturer, submit the repository link on ARC, and include a screenshot of the green CI run.

## Video presentation
Record an unlisted YouTube video in your own voice. Demonstrate the running API in Swagger, explain the code structure and EF Core model, register/login as both roles, show permitted and rejected role access, demonstrate event/category/enrolment/result endpoints, run unit tests, and show the successful GitHub Actions workflow. Add the actual unlisted link here:

`[PASTE YOUR UNLISTED YOUTUBE LINK HERE]`

## AI-use disclosure
The POE says AI use for planning, proofreading or coding must be disclosed briefly in a footnote or comments section. Adapt this disclosure honestly to describe your actual use and what you changed/verified.

## Part 2 alignment note
This implementation follows the RaceDay resources and role names from the Part 1 plan available in the project context. Before submitting, compare every route and field against your own committed `docs/API_Endpoint_Plan.md` and `docs/RaceDayDB.sql`. Part 3 will depend on this API contract.
