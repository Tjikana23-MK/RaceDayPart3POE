# Part 2 test checklist

Run through this checklist against the live API in Swagger and record your actual results/screenshots.

1. Register an Organiser and a Participant; verify the response does not contain a password or password hash.
2. Attempt duplicate registration using the same email; expect `409 Conflict`.
3. Login with a correct password; expect success and an authenticated session. Try a wrong password; expect `401 Unauthorized`.
4. As an unauthenticated browser session, call `POST /api/events`; expect `401`.
5. As a Participant, call `POST /api/events`; expect `403`.
6. As an Organiser, create an event using event type ID 1, 2 or 3; expect `201`.
7. As another Organiser, attempt to edit/delete the first Organiser's event; expect `403`.
8. Create at least one category for the event; list the categories publicly.
9. As a Participant, enrol using a valid event/category pair; expect `201`. Repeat; expect `409`. Try a category belonging to a different event; expect `400`.
10. As the Organiser who owns the event, view its enrolments and record a result; expect success. A different Organiser must be forbidden.
11. As the Participant, view `/api/results/mine` and confirm only their own result is returned.
12. Run `dotnet test RaceDay.sln --configuration Release` and capture the actual output.
13. Push to GitHub and capture the actual successful Actions run.

For each failure case, save a screenshot showing the request, status code and response. Do not mark a test passed unless you executed it.
