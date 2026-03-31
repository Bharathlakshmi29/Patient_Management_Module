@echo off
echo Running database migration to add AnalysisResult column...
cd /d "d:\Patient_Management_Module\Patient_Management_Module\Patient_mgt.Data"
dotnet ef database update --startup-project "..\Patient_Management_Module"
echo Migration completed!
pause