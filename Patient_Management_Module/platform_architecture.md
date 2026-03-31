# Patient Management Module
## AI-Enabled EMR Management Platform

### 1. Executive Summary
The Patient Management Module is a full-stack healthcare web application designed to support medical clinics in managing their patient's Electronic Medical Records (EMRs).
The platform provides a centralized system for:
*   Patient registration and demographic lifecycle management
*   Electronic Medical Records (EMR) tracking
*   Insurance profile management
*   Doctor & Staff directory administration
*   AI-assisted laboratory report processing
The system integrates modern web technologies, secure authentication, and AI-powered document extraction to streamline clinical workflows and reduce manual data entry.
The architecture follows a layered microservice design, separating the frontend, backend API, database, and integrated AI services to ensure scalability and maintainability.

### 2. Project Objectives
The system is designed to address key operational challenges in modern medical clinics.

**Primary Objectives**
*   Centralize Patient and Doctor management
*   Reduce manual data entry through AI automation (OCR + LLM)
*   Provide robust Role-Based Access Control (RBAC) for clinical staff
*   Improve visibility into patient histories and diagnostics
*   Support structured clinical record keeping
*   Enable fast, AI-assisted clinical decision making

### 3. Technology Stack

| Layer | Technology |
| :--- | :--- |
| **Frontend** | Angular 17+, TypeScript, Bootstrap/Tailwind |
| **Backend** | ASP.NET Core REST API (.NET 8) |
| **ORM** | Entity Framework Core |
| **Database** | SQL Server |
| **Cloud Storage**| Cloudinary |
| **AI Extraction**| OCR API Provider |
| **AI LLM** | Google Gemini API (`IGeminiService`) |
| **Medical Coding**| ClinicalTables API (ICD-10) |
| **Authentication**| JWT Bearer Tokens |

### 4. System Architecture
The platform follows a standard 3-tier architecture integrated with advanced AI processing pipelines.

**High-Level Architecture Diagram**
```text
             +------------------------+ 
             |   Angular Frontend     | 
             |   (Web Browser)        | 
             +-----------+------------+ 
                         | 
                         | HTTP REST + JWT 
                         | 
             +-----------v------------+ 
             |  ASP.NET Core API      | 
             |                        | 
             | Controllers            | 
             | Services               | 
             | Repositories           | 
             +-----------+------------+ 
                 /       |        \
    Entity Framework     |        External Integrations
               /         |          \
 +-----------v---------+ |   +-------v--------+ +-------v------+
 |     SQL Server      | |   |  Cloudinary    | | OCR Provider |
 |     Database        | |   |  (Storage)     | | (Extraction) |
 +---------------------+ |   +----------------+ +--------------+
                         |
                 +-------v--------+ +-------v---------+
                 | Google Gemini  | | ClinicalTables  |
                 | (AI Analysis)  | | (ICD-10 Coding) |
                 +----------------+ +-----------------+
```
The remote services operate dependently through the Backend API orchestrator when processing patient features and uploaded lab reports.

### 5. Backend Architecture Pattern
The backend follows a strict layered architecture pattern.

```text
Controller Layer
       ↓
Service Layer (Business Logic)
       ↓
Data Access / Repository Layer
       ↓
Entity Framework Core
       ↓
SQL Server
```

**Benefits of this design:**
*   Separation of concerns
*   Testable business logic
*   Loose coupling through dependency injection interfaces (`IGeminiService`, `ICloudinaryService`)
*   Easier maintainability and scaling
All dependencies are registered through ASP.NET Dependency Injection container.

### 6. Role-Based Access Control
The system uses Role-Based Access Control (RBAC) implemented through:
*   JWT Tokens with specific Claims
*   ASP.NET Request Authorization Policies
*   Angular Route `AuthGuards` and `RoleGuards`

| Role | Responsibilities |
| :--- | :--- |
| **Admin** | Full access: User management, doctor/staff profiles, analytics, and overall platform administration. |
| **Doctor** | Clinical care: Viewing patient histories, managing EMRs, and reviewing AI-extracted lab results. |
| **Staff** | Operations: Patient registration, updating demographics, managing insurance, and uploading medical reports. |

Security enforcement occurs actively at both the SPA frontend and the API backend layers.

### 7. Core System Modules

#### 7.1 Patient Management
Handles the full lifecycle of medical patients.
**Features include:**
*   Comprehensive registration components
*   Patient searching via MRN/ID
*   Full CRUD operations (Create, Read, Update, Delete)
**Patient data includes:**
*   Demographics (Name, Age, Address, Contact)
*   Emergency contacts
*   Underlying medical conditions

#### 7.2 Doctor & Staff Management
Manages the hospital's clinical administrators and physicians.
**Information recorded:**
*   Specializations
*   Contact details
*   Assigned platform User IDs

#### 7.3 Electronic Medical Records (EMR)
Tracks all active clinical notes, diagnostics, and historical treatments.
**Features:**
*   Creation of encounter notes by Doctors
*   Assignment of globally recognized **ICD-10** codes
*   Global, Doctor-specific, or Patient-specific retrieval streams

#### 7.4 Insurance Management
Stores patient insurance and billing authorization records.
**Information includes:**
*   Provider name
*   Policy numbers
*   Coverage validity dates

#### 7.5 AI-Assisted Medical Reports
The core module automating the extraction of laboratory diagnostic values from raw uploads.

### 8. AI-Assisted Lab Upload System
The architecture module automates extraction of critical medical values from unstructured image/PDF files using Google Gemini.

**AI Processing Pipeline:**
```text
Staff Uploads Lab Report (PDF/Img)
       |
       v
Angular Upload Component
       |
       v
ASP.NET API
[ POST /api/reports/upload ]
       |
       +-----------------------------------+
       |                                   |
       v                                   v
 Cloudinary Storage                  OCR Text Extraction
 (Returns Secure URL)              (Returns Raw String Text)
                                           |
                                           v
                                  IGeminiService (Google Gemini)
                                 (Applies Medical Prompt Design)
                                           |
                                           v
                                 Structured JSON Output 
                                 & Clinical Summary
```

**Example AI Output Mapping:**
```json
{
  "testResults": [
    { "testName": "Creatinine", "value": "0.9", "unit": "mg/dL", "referenceRange": "0.7 - 1.3", "isAbnormal": false },
    { "testName": "Hemoglobin", "value": "11.2", "unit": "g/dL", "referenceRange": "13.8 - 17.2", "isAbnormal": true }
  ],
  "clinicalSummary": "Patient exhibits low hemoglobin levels indicative of potential anemia. Creatinine is normal."
}
```

### 9. AI Data Flow
```text
Staff Member Uploads File
       |
       v
Angular Frontend
       |
       v
ASP.NET Backend
       |
       v
OCR Provider + Cloudinary Storage
       |
       v
Google Gemini LLM Analysis
       |
       v
Structured Lab Values + Flagged Abnormalities
       |
       v
SQL Database Persistence
       |
       v
Doctor Reviews Results in EMR Dashboard
```

### 10. Authentication Architecture
Authentication is implemented securely via JSON Web Tokens.

**Login Flow:**
```text
User Submits Credentials
       |
       v
POST /api/auth/login
       |
       v
Backend Validator Extrapolates User Role
       |
       v
JWT Generated with specific Auth Claims (Admin/Doctor/Staff)
       |
       v
Angular stores Token (LocalStorage/Session)
       |
       v
HTTP AuthInterceptor attaches Bearer to Headers
```
Angular route guards subsequently validate UI access automatically.

### 11. Database Design
Key entity relationships include:
| Entity | Description |
| :--- | :--- |
| **User** | Core system identity (Admin, Staff, Doctor) |
| **Patient** | Core demographic information |
| **Doctor** | Physician metadata mapped to a User ID |
| **EMR** | Electronic Medical Record instance bridging Patient & Doctor |
| **Insurance**| Insurance provider profiles |
| **MedicalReport**| Records mapped to cloud storage and AI extracted data |
