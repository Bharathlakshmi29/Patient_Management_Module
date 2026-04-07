# Patient Management Module — Project Documentation

---

## 1. What Has Been Built or Configured

### Full-Stack Application Architecture
A layered, full-stack healthcare web application with:
- **Frontend**: Angular (SSR-enabled) SPA
- **Backend**: ASP.NET Core REST API (.NET 8)
- **Database**: PostgreSQL accessed via Entity Framework Core
- **Python RAG Microservice**: FastAPI service running on `http://127.0.0.1:8000` serving vector-based document retrieval

---

### Backend — ASP.NET Core API

#### Solution Structure (Clean Architecture)
| Project | Responsibility |
|---|---|
| `Patient_mgt.Domain` | Entity models, enums |
| `Patient_mgt.DTOs` | Data Transfer Objects |
| `Patient_mgt.Application` | Repository interfaces |
| `Patient_mgt.Data` | EF Core DbContext, migrations, repositories |
| `Patient_mgt.Infrastructure` | Services, external integrations |
| `Patient_mgt.Mappings` | AutoMapper profiles |
| `Patient_Management_Module` | API entry point, controllers, middleware |

#### Domain Entities Built
- `Patient` — demographics, MRN, blood group, photo URL, status (Stable/Mild/Critical)
- `User` — authentication identity with role (Admin, Staff, Doctor)
- `Doctor` — linked to User, specialization, department enum (15 departments)
- `EMR` — visit records with ICD-10 code, diagnosis, notes, existing conditions
- `PrescribedMedicine` — linked to EMR, dosage, frequency, instructions
- `Insurance` — patient insurance policies
- `MedicalReport` — file metadata, Cloudinary URL, AI analysis result JSON, report type enum

#### API Controllers
- `PatientController` — full CRUD, MRN lookup, Excel export
- `DoctorController` — full CRUD, doctor directory
- `EMRController` — EMR creation, retrieval by patient/doctor
- `MedicalReportController` — upload, download, delete, `can-analyze` pre-check endpoint
- `InsuranceController` — insurance CRUD per patient
- `IcdController` — ICD-10 code search (proxies ClinicalTables NLM API)
- `OcrController` — OCR text extraction trigger
- `ChatController` — AI chatbot endpoint (`GET /api/Chat/ask`)
- `UserController` — user/staff management
- `TokenController` — JWT token issuance

#### Security Configuration
- JWT Bearer authentication with symmetric key signing
- Role-based authorization: `Admin`, `Doctor`, `Staff`
- Swagger UI configured with Bearer token authorization button
- CORS policy configured for Angular dev server (`http://localhost:4200`)
- `GlobalExceptionMiddleware` for centralized error handling
- `LoggingMiddleware` for request/response logging
- JSON cycle reference handling (`ReferenceHandler.IgnoreCycles`)

#### External Service Integrations
| Service | Purpose | Implementation |
|---|---|---|
| Google Gemini API | Lab data extraction + clinical summary + RAG LLM responses | `GeminiService` with multi-model fallback chain |
| Cloudinary | Secure cloud storage for images and PDFs | `CloudinaryService` with separate image/document upload paths |
| OCR.space API | Text extraction from lab report images/PDFs | `OcrService` with PDF/PNG/JPEG auto-detection |
| ClinicalTables NLM | ICD-10-CM code search | `IcdService` via typed `HttpClient` |
| Python RAG API | Vector similarity search over medical guidelines | `RagService` via typed `HttpClient` |

---

### AI / RAG Pipeline

#### Hybrid RAG Chatbot
A three-component pipeline orchestrated by `ChatService`:

1. **QueryRouter** — classifies every incoming question into one of three intents:
   - `PatientData` — queries about a specific patient (by name or MRN)
   - `Guideline` — general medical knowledge questions
   - `Hybrid` — patient-specific question that also needs clinical guidelines

2. **PatientDataService** — when patient intent is detected, fetches structured context from PostgreSQL using Dapper (raw SQL): last 5 EMR visits, last 10 prescribed medicines, last 5 medical reports

3. **RagService** — calls the Python pgvector microservice (`GET /query?q=...`) to retrieve semantically relevant medical guideline chunks

4. **GeminiService.GenerateResponse** — assembles a structured prompt with patient data and/or guideline context sections, then calls Gemini to generate a grounded answer

#### AI Lab Report Analysis Pipeline
When a lab report is uploaded:
1. File stored to Cloudinary → secure URL returned
2. OCR service extracts raw text from the file
3. Gemini `ExtractLabTestsAsync` parses text into structured `LabTestResult` objects (test name, value, unit, reference range, `isAbnormal` flag)
4. Gemini `GenerateClinicalSummaryAsync` generates a formatted clinical summary
5. Results and summary stored as JSON in `MedicalReport.AnalysisResult`

Gemini is configured with a **multi-model fallback chain**: `gemini-2.5-flash` → `gemini-2.0-flash` → `gemini-flash-latest` → `gemini-2.5-flash-lite` → `gemini-2.0-flash-001`, skipping rate-limited models automatically.

---

### Frontend — Angular

#### Core Infrastructure
- **HTTP Interceptor** (`authinterceptorInterceptor`) — attaches `Authorization: Bearer <token>` to every request except `/token`
- **AuthGuard** — redirects unauthenticated users to `/login`; SSR-aware (bypasses guard on server)
- **RoleGuard** — restricts routes by user role

#### Feature Modules
| Module | Components |
|---|---|
| Auth | Login |
| Dashboard | Dashboard with charts (`chart.config.ts`) |
| Patient | Patient list, add, update, details, medical report list |
| Doctor | Doctor list, edit doctor |
| EMR | EMR management |
| Insurance | Insurance management |
| Staff | Add staff |
| Profile | Edit profile |
| Landing | Landing page |

#### Angular Services (Core)
`AuthService`, `PatientService`, `DoctorService`, `EMRService`, `InsuranceService`, `MedicalReportService`, `LabReportService`, `IcdService`, `ChatService`, `UserService`, `StorageService`, `LocationService`

---

### Database

- PostgreSQL with EF Core migrations (`InitialCreate`)
- `Npgsql.EnableLegacyTimestampBehavior` enabled for UTC timestamp compatibility
- Unique indexes on `Patient.Phone` and `User.EmailId`
- Cascade delete configured: Patient → Insurance, Patient → MedicalReport, Patient → EMR, EMR → PrescribedMedicine
- Restrict delete on Doctor → EMR (prevents orphaned records)

---

### CI/CD & Infrastructure
- `.github/workflows/` — GitHub Actions pipeline configured
- `run_migration.bat` — script to apply EF Core migrations
- `architecture_diagram.html` + `architecture.mmd` — visual architecture diagrams

---

## 2. What Has Been Learned and Applied Technically

### Clean Architecture in .NET
Separating Domain, Application (interfaces), Data (EF), Infrastructure (services), and API layers enforces single responsibility and makes each layer independently testable. Dependency injection wires everything together through interfaces (`IPatientService`, `IRagService`, etc.) without tight coupling.

### Hybrid RAG Pattern
Learned to combine two knowledge sources — a structured SQL database and an unstructured vector store — in a single query pipeline. The key insight is routing: not every question needs both sources. A `QueryRouter` using keyword matching and regex extracts intent, patient name, and MRN from natural language before any expensive I/O happens.

### Prompt Engineering for Grounded Responses
The `BuildPrompt` method in `ChatService` enforces strict grounding: the LLM is explicitly instructed not to use its training data and to answer only from the provided context sections (`PATIENT DATA`, `MEDICAL GUIDELINES`). This prevents hallucination in a clinical context.

### Multi-Model LLM Fallback
Rather than failing when a single Gemini model is rate-limited, the service iterates through a prioritized model list and continues to the next on `429 TooManyRequests`. This pattern keeps the service available under quota pressure without manual intervention.

### OCR + LLM for Unstructured Document Processing
Learned the two-step pipeline: OCR converts a binary file (PDF/image) to raw text, then an LLM with a structured prompt converts that raw text into typed JSON. A regex-based fallback parser (`ParseLabResultsBasic`) handles cases where the LLM fails, ensuring the pipeline degrades gracefully.

### Dapper for Read-Heavy Queries
`PatientDataService` uses Dapper (raw SQL) alongside EF Core. This is applied specifically for the chatbot's context-building queries where performance matters and the result is a formatted string, not a tracked entity — a practical example of choosing the right data access tool per use case.

### Angular SSR Guard Handling
The `AuthGuard` checks `isPlatformServer(platformId)` and returns `true` on the server side. Without this, Angular Universal SSR would fail trying to access `localStorage` during server-side rendering, since `localStorage` is a browser-only API.

### JWT with Role Claims
JWT tokens carry role claims that are validated at both layers: ASP.NET `[Authorize(Roles = "...")]` attributes on controllers enforce server-side access, while Angular `RoleGuard` enforces the same rules on the client-side routing — defense in depth.

### Cloudinary for Medical File Storage
Learned the distinction between `ImageUploadParams` (for photos) and `RawUploadParams` (for PDFs/documents). The `Type = "upload"` and `AccessMode = "public"` settings on raw uploads are required to make documents publicly accessible via their secure URL — a non-obvious Cloudinary configuration detail.

### EF Core Relationship Configuration
Configured cascade vs. restrict delete behaviors explicitly in `OnModelCreating` rather than relying on defaults. Cascade on patient-owned data (reports, insurance, EMRs) and restrict on doctor-to-EMR prevents accidental data loss when a doctor record is removed.

### pgvector Integration via Python Microservice
Rather than embedding vector search directly in .NET, a Python FastAPI service handles pgvector operations. The .NET `RagService` communicates with it over HTTP. This separation keeps the ML/vector tooling in Python (where the ecosystem is richer) while the main application logic stays in .NET.
