# Functional Requirements Document
## Project: Patient Management Module

### 1. Introduction
The **Patient Management Module** is an Electronic Medical Records (EMR) system designed to manage patients, doctors, medical staff, insurance details, and medical/lab reports. The system is built with an Angular frontend and a .NET Core web API backend. It features role-based access control and integrates AI capabilities (Google Gemini and OCR) for automated lab report analysis.

#### 1.1 Project Objective
The primary objective of this module is to digitize and streamline clinical and hospital administrative workflows. It aims to reduce manual data entry errors, accelerate the time doctors take to analyze complex lab reports using AI extraction, and provide a secure, centralized repository for all patient medical histories and diagnostic data.

### 2. User Roles & Access Control
The application enforces Role-Based Access Control (RBAC) with the following primary roles:
*   **Admin**: Full access. Can manage users, staff, doctors, and oversee the entire system.
*   **Staff**: Operational access. Can register and manage patient records, and upload medical reports.
*   **Doctor**: Clinical access. Can view patient details, manage EMRs, and view/analyze lab/medical reports.

---

### 3. Core Functional Modules

#### 3.1 Authentication & User Management
*   **Login**: Users must authenticate to access the system. The system maintains authentication state (handling page refreshes and SSR).
*   **Profile Management**: Users can edit their personal profile (`EditProfileComponent`).
*   **Staff/User Management**:
    *   Admins can view all users in the system.
    *   Admins can create new user accounts (Add Staff/Admin functionality).
    *   Admins can update and delete user credentials and role assignments.

#### 3.2 Patient Management
*   **Patient Registration**: Authorized users (Staff, Admin) can create new patient profiles, capturing essential demographic and medical details.
*   **Patient Directory**: Users can view a list of all patients.
*   **Patient Search/Details**: Users can retrieve and view specific patient details using their ID or Medical Record Number (MRN).
*   **Patient Updates**: Staff and Admins can update existing patient information.
*   **Patient Deletion**: Staff and Admins can delete patient records.
*   **Data Export**: Authorized users can export the patient list to an Excel spreadsheet (`.xlsx`).

#### 3.3 Doctor Management
*   **Doctor Directory**: Users can view a list of all doctors.
*   **Doctor Details**: Retrieve doctor information by their Doctor ID or underlying User ID.
*   **Doctor Administration**: Admins can add new doctors, update their information, or remove them from the system.
*   **Data Export**: The list of doctors can be exported to an Excel spreadsheet.

#### 3.4 Electronic Medical Records (EMR)
*   **EMR Creation**: Doctors can create new EMR entries detailing patient visits, diagnoses, and treatments. The system supports tracking **Existing Conditions** for patients.
*   **ICD-10 Integration**: Doctors can search for and assign standard ICD-10-CM codes for diagnoses, powered by external health API integration.
*   **EMR Retrieval**: EMRs can be fetched globally, by specific EMR ID, by Patient ID (to see a patient's history), or by Doctor ID (to see a doctor's work history).
*   **EMR Modification**: Authorized users can update or delete existing EMR entries.

#### 3.5 Insurance Management
*   **Insurance Registration**: Users can add insurance details for patients.
*   **Insurance Lookup**: Retrieve all insurance policies associated with a specific Patient ID.
*   **Manage Policies**: Update or delete insurance records as policies change or expire.

#### 3.6 Medical & Lab Reports (AI-Powered)
*   **Upload & Storage**: Staff can upload medical and lab reports for a patient (images or PDFs). Files are securely stored in the cloud using **Cloudinary**.
*   **Report Management**: Users can view, update, download, and delete uploaded medical reports.
*   **AI Lab Report Analysis**:
    *   **Pre-check**: The system determines if a report is eligible for analysis (e.g., `LAB_REPORT`, `BLOOD_TEST`, `URINE_TEST`).
    *   **OCR Extraction**: The system uses an OCR service to extract raw text from the uploaded report files.
    *   **Data Structuring**: The system utilizes **Google Gemini LLM** to parse the extracted text into structured lab test results (Test Name, Value, Unit, Reference Range, and an `IsAbnormal` flag).
    *   **Clinical Summary**: Gemini LLM generates a cohesive clinical summary based on the extracted lab results.
    *   **Storage & Display**: The structured abnormal/normal results and clinical summary are saved to the database and displayed to the Doctor/User for clinical decision-making.

### 4. System Architecture Requirements
*   **Frontend**: Angular Application with route guards protecting authenticated and role-specific views.
*   **Backend**: .NET Core RESTful API.
*   **Database**: Relational database accessed via Entity Framework Core.
*   **External Integrations**:
    *   Google Gemini API (via `IGeminiService` for AI analysis)
    *   OCR Provider (via `IOcrService` for text extraction)
    *   Cloudinary API (via `ICloudinaryService` for secure file storage)
    *   ClinicalTables ICD-10 API (via `IIcdService` for diagnosis coding)

### 5. Architectural Flow & Detailed Explanation

The system follows a typical 3-tier architecture with an integrated AI-driven processing pipeline for medical reports. 

#### 5.1 High-Level Architecture
1. **Presentation Layer (Frontend)**: Developed in Angular, providing an interactive, responsive User Interface. It communicates with the backend via HTTPS REST APIs. Authentication details (JWT tokens) are managed securely to persist user sessions.
2. **Business Logic Layer (Backend API)**: Developed in ASP.NET Core. It handles authentication, authorization, data validation, and core business rules. It acts as the orchestrator between the database repository and external service configurations.
3. **Data Access Layer (Database)**: An SQL Server (or similar Relational Database) managed via Entity Framework Core (ORM) for persistent storage of user credentials, patient information, EMRs, and lab results.
4. **Third-Party Integrations Layer**: External services leveraged for advanced processing: file storage (Cloudinary), medical coding data (ClinicalTables ICD-10), text extraction (OCR), and complex AI medical language processing (Google Gemini).

#### 5.2 Architectural Flow Diagram

```text
+---------------------------------------------------------+
|                 FRONTEND (Angular Client)               |
|                                                         |
|  [ User Interface ] <---> [ Auth/Role Guards ]          |
|          |                                              |
+----------|----------------------------------------------+
           | REST API Calls + JWT
           v
+---------------------------------------------------------+
|                  BACKEND (.NET Core API)                |
|                                                         |
|  [ API Endpoints (Gateway) ]                            |
|          |                                              |
|          v                                              |
|  [ API Controllers ]                                    |
|          |                                              |
|          v                                              |
|  [ Business Services Layer ]                            |
|    |      |      |      |                               |
|    |      |      |      +-----> [ Entity Framework ]    |
|    v      v      v                    |                 |
| [OCR] [Cloud] [Gemini] [ICD-10]       |                 |
+----|------|------|--------|-----------|-----------------+
     |      |      |        |           |
     |      |      |        |           v
     |      |      |        |    +--------------------+
     |      |      |        |    | DATABASE (SQL DB)  |
     |      |      |        |    +--------------------+
     v      v      v        v
+---------------------------------------------------------+
|                 EXTERNAL API PROVIDERS                  |
|                                                         |
|  [ OCR Provider ]     [ Cloudinary Cloud Storage ]      |
|  [ Google Gemini API] [ ClinicalTables API (ICD-10) ]   |
+---------------------------------------------------------+
```

#### 5.3 Detailed Request Flow Explanation
1. **User Interaction & Auth**: Users (Doctor/Staff/Admin) interact with the Angular UI. Upon login, the API issues a JWT token. The Angular application stores this token and attaches it via HTTP interceptors to all subsequent protected API requests. `AuthGuards` and `RoleGuards` restrict UI access based on privileges.
2. **Standard Data Operations**: Basic functionalities (e.g., creating a patient, updating an EMR record) are transmitted as JSON payloads. The .NET API controllers validate the payload, enforce role authorization, process the business logic in the Service layer, and interact with the SQL Database through Entity Framework.
3. **Automated AI Report Analysis Flow**:
   - **File Upload**: A Staff member uploads a lab report (PDF/Image). The Angular client transmits the file as `multipart/form-data` to the .NET backend.
   - **Cloud Storage**: The API's `ICloudinaryService` uploads the file to **Cloudinary** and retrieves a secure, permanent file URL.
   - **Text Extraction**: The `IOcrService` sends the file URL or blob to an **OCR Provider** to extract the raw, unstructured text.
   - **AI Structuring & Analysis**: The extracted text is then transmitted to the **Google Gemini API** via the `IGeminiService`. Following a custom prompt design, Gemini intelligently parses the raw text and returns standard JSON-formatted structured lab results (Test Names, Values, Reference Ranges) alongside a generated clinical summary.
   - **Persistence & Alerting**: The API checks the structured results, flags any out-of-range/abnormal values, and persists the data (and summary) linked to the Patient into the SQL Database.
   - **Clinical Display**: Doctors viewing the patient's EMR can see the extracted lab data seamlessly, aiding in an accurate, AI-assisted diagnosis.

### 6. Data Flow Summary
The core lifecycle of data within the system follows this sequential pattern:
1. **Ingestion**: Patient details, insurance profiles, and medical files are ingested by clinical Staff through Angular Web Forms. 
2. **Sanitization & Auth Check**: All data inputs hit the .NET Controllers, where the user's role token and validation constraints are verified before processing.
3. **Delegation**: 
    - *Structured Data* (Demographics, conditions, standard ICD-10 links) are immediately routed to Entity Framework for standard SQL storage.
    - *Unstructured Data* (Lab upload files) embark on the AI integration pipeline, generating supplementary records for extracted metrics.
4. **Retrieval & Action**: Doctors request EMR data; the API joins native SQL data with generated AI summaries and serves the unified clinical picture to the Angular Client for final medical review.
