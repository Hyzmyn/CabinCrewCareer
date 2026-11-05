# CabinCrewCareer – Backend API

Recruitment and promotion platform for flight attendants: airlines run hiring campaigns, candidates apply, take online exams (Listening, Speaking, Practical), go through interview rounds, and receive results by email.

Graduation project, FPT University (Fall 2025). Team of 5: 3 backend, 2 frontend.

> This is a portfolio copy of the team repository. Every credential was removed from the git history before publishing. To run it, supply your own configuration (see below).

## Tech stack

- **.NET 8**, ASP.NET Core Web API
- **Clean Architecture** (Domain / Application / Infrastructure / Api) with **CQRS** via MediatR
- Entity Framework Core, SQL Server
- FluentValidation, AutoMapper, Swagger
- JWT authentication with refresh tokens
- Firebase Storage and Cloudinary for files and audio, MailKit for email
- EPPlus / ClosedXML for Excel import
- xUnit tests (`tests/CabinCrewCareer.Application.UnitTests`)
- GitHub Actions workflow to Azure App Service

## Project structure

```
src/
  CabinCrewCareer.Domain/          entities and domain rules
  CabinCrewCareer.Application/     commands, queries, handlers, validators (one folder per feature)
  CabinCrewCareer.Infrastructure/  EF Core, external services (storage, email, OCR)
  CabinCrewCareer.Api/             controllers, authentication, Swagger
tests/
  CabinCrewCareer.Application.UnitTests/
```

Main features: campaigns and campaign rounds, applications, airline partners and requirements, tests and test sessions, scoring criteria, interview criteria and results, enquiry (re-marking) requests, notifications, dashboard.

## My part (Trần Duy Hưng, [@Hyzmyn](https://github.com/Hyzmyn))

### Team leader

Much of this work does not show up as commits:

- Planned the backlog and assigned tasks across the backend and frontend teams; tracked progress to the final defense.
- Set up and managed the team's database environments (local and deployed) and coordinated schema changes with the backend team.
- Defined the requirements and flows for the AI document checks (ID cards and TOEIC certificates via Azure OCR and Gemini), implemented by [@NguyenHNN](https://github.com/NguyenHNN).
- Supported both sides of the stack: helped debug frontend–API integration and backend issues, and reviewed teammates' work before merge.

### Hands-on: online exam module

I owned the **online exam module** end to end:

- APIs for tests, test questions and test sessions: create exams, time limits, serve questions to candidates, submit answers, score.
- Listening, Speaking and Practical exam types; audio upload and storage on Firebase Storage and Cloudinary.
- Question bank import from Excel.
- Speaking scoring API for examiners and the **enquiry request** (re-marking) flow.
- Automated scoring and **round progression**: scores are checked against the benchmark configured for each round; passing candidates move to the interview round, round history is recorded and result emails are sent automatically.
- Flight-hour validation against each partner airline's requirements.

## Running locally

Requirements: .NET 8 SDK, SQL Server.

`appsettings.json` ships with empty values. Provide real ones through [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables, for example:

```bash
cd src/CabinCrewCareer.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your SQL Server connection string>"
dotnet user-secrets set "JwtSettings:Secret" "<at least 32 random characters>"
```

| Setting | Used for |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server database |
| `JwtSettings:Secret` | signing access tokens |
| `Firebase:ApiKey`, `Firebase:StorageBucket` | audio and file storage |
| `CloudinarySettings:CloudName`, `ApiKey`, `ApiSecret` | media storage |
| `EmailSettings:Email`, `Password` | sending result emails (Gmail app password) |
| `AzureOcr:ApiKey`, `Gemini:ApiKey` | document checks (ID card, certificates) |

Then:

```bash
dotnet ef database update --project src/CabinCrewCareer.Infrastructure --startup-project src/CabinCrewCareer.Api
dotnet run --project src/CabinCrewCareer.Api
```

Swagger UI is available at `/swagger`.

## Team

Backend: [@thanhnhan151](https://github.com/thanhnhan151), [@NguyenHNN](https://github.com/NguyenHNN), [@Hyzmyn](https://github.com/Hyzmyn)
Frontend: [@benzhoang](https://github.com/benzhoang), [@HoangTranNhat](https://github.com/HoangTranNhat) — [CabinCrew_Web](https://github.com/benzhoang/CabinCrew_Web)

Original repository: [thanhnhan151/CabinCrewCareer](https://github.com/thanhnhan151/CabinCrewCareer)
