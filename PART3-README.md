# RaceDay — Part 3 (PROG6212)

ASP.NET Core MVC front end consuming the Part 2 REST API. The MVC project has no Entity Framework or database connection. It uses server-side session state and forwards the API authentication cookie to the API.

## Included
- Participant: upcoming events, event/category details, enrolment, My Events, My Results, profile update.
- Organiser: dashboard, event create/delete, category management, enrolments, result entry/update.
- Bootstrap responsive RaceDay navy/blue/orange theme.
- API-only media endpoints for event banners and participant profile images using Azure Blob Storage.
- Dockerfile, Docker Compose, GitHub Actions build/test workflow.

## Important compatibility notes
The supplied Part 2 API stores authentication in the `.RaceDay.Auth` cookie. MVC stores that API cookie in its own session and sends it on subsequent API requests. Keep API and MVC reachable from each other. `ApiSettings:BaseUrl` must point to the API address visible from the MVC process.

The supplied Part 2 schema has no `BannerUrl` or `ProfileImageUrl` fields. The media endpoints therefore upload to Azure and return a blob URL, but the current schema does not persist that URL against an event/user or automatically display it. To complete persistent image display, add nullable `BannerUrl` to Event and `ProfileImageUrl` to User in Part 2, create/apply an EF migration, and update the media controller to save the returned URL. Do not create a second database in MVC. Azure Storage connection string is configured only in the API using `AzureBlobStorage__ConnectionString`. The implementation creates a `raceday-media` container with blob-level public access so the browser can render uploaded images; ensure your Azure Storage account permits public blob access, or replace this with short-lived read SAS URLs for a private container.

The Part 2 ProfileController currently returns only FirstName (not LastName) and its update implementation ignores LastName; correct that existing API DTO/controller to persist both names if the database schema has the LastName column. The MVC profile form sends both fields as the planned API contract.

## Local run (Visual Studio 2022)
1. Open `RaceDay.sln`.
2. Set `RaceDay.Api` as startup project and run it. Confirm Swagger opens and the API can connect to your existing SQL Server `RaceDayDB`.
3. In `RaceDay.Mvc/appsettings.json`, set `ApiSettings:BaseUrl` to the API URL shown in the browser (include trailing slash).
4. Right-click `RaceDay.Mvc` > Set as Startup Project and run. Register as Organiser and Participant to test both menus.
5. Set Azure connection string as an environment variable on the API host: `AzureBlobStorage__ConnectionString`. Never commit the real key to GitHub.

## Docker
For an API+MVC deployment, configure `.env` (do not commit it) with `RACEDAY_DB_CONNECTION` and `AZURE_BLOB_CONNECTION`, then run:

```bash
docker compose up --build
```

Open MVC at `http://localhost:8080`; API at `http://localhost:5000`. The SQL Server host must be reachable from the API container. On Docker Desktop, SQL Server running on the host may be referenced with `host.docker.internal` in the connection string.

MVC-only image build/run (requires an already-running API reachable from container):

```bash
docker build -t raceday-mvc -f Dockerfile .
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5000/ raceday-mvc
```

## Build and test
```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln --configuration Release
dotnet test RaceDay.Tests/RaceDay.Tests.csproj --configuration Release
```

## GitHub and submission checklist
- Commit each meaningful completed feature in your own GitHub account; the assessment requires at least 20 meaningful commits. Do not manufacture commits; commit actual incremental work as you complete and test it.
- Push the Part 3 changes and confirm the Actions workflow is green.
- Add screenshots of both role experiences and successful CI to the README.
- Record the required unlisted YouTube demonstration in your own voice, including MVC/API flow, role-based navigation, image upload configuration, and Docker.
- Submit the GitHub repository and video link through ARC.

## Known integration work to verify against your live Part 2 API
The source ZIP contains API routes for auth, events, categories, enrolments, results and profile. Run API Swagger and validate all routes before the demonstration. Blob upload endpoints are added in `MediaController`; they require the Azure package restore and a configured storage account. The uploaded URLs are returned by the API but persistence/display requires the schema change described above. Profile picture persistence similarly requires adding the profile image URL field.
