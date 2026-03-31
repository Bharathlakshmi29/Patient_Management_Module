using AutoMapper;
using Patient_mgt.Application;
using Patient_mgt.Domain;
using Patient_mgt.DTOs;

namespace Patient_mgt.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Patient → PatientDTO
            CreateMap<Patient, PatientDTO>()
                .ForMember(dest => dest.PatientId,
                    opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.MRN,
                    opt => opt.MapFrom(src => src.MRN))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // CreatePatientDTO → Patient
            CreateMap<CreatePatientDTO, Patient>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => Enum.Parse<PatientStatus>(src.Status)));

            // Patient → GetPatientDTO
            CreateMap<Patient, GetPatientDTO>()
                .ForMember(dest => dest.PatientId,
                    opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.MRN,
                    opt => opt.MapFrom(src => src.MRN))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PhotoUrl,
                    opt => opt.MapFrom(src => src.PhotoUrl));

            // User mappings
            CreateMap<User, UserDTO>();
            CreateMap<CreateUserDTO, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (UserRole)src.Role));
            CreateMap<User, GetUserDTO>();

            // Doctor mappings
            CreateMap<Doctor, DoctorDTO>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User.EmailId))
                .ForMember(dest => dest.Department,
                    opt => opt.MapFrom(src => src.Department.ToString()));

            CreateMap<CreateDoctorDTO, Doctor>()
                .ForMember(dest => dest.Department,
                    opt => opt.MapFrom(src => Enum.Parse<Department>(src.Department)));

            CreateMap<Doctor, GetDoctorDTO>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User.EmailId))
                .ForMember(dest => dest.Department,
                    opt => opt.MapFrom(src => src.Department.ToString()));

            // EMR mappings
            CreateMap<EMR, EMRDTO>();
            CreateMap<CreateEMRDTO, EMR>()
                .ForMember(dest => dest.PrescribedMedicines, opt => opt.Ignore());
            CreateMap<PrescribedMedicine, GetPrescribedMedicineDTO>();
            CreateMap<CreatePrescribedMedicineDTO, PrescribedMedicine>();

            // Insurance mappings
            CreateMap<Insurance, GetInsuranceDTO>()
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src => src.Patient.FirstName + " " + src.Patient.LastName));
            CreateMap<CreateInsuranceDTO, Insurance>();
            CreateMap<UpdateInsuranceDTO, Insurance>();

            // Medical Report mappings
            CreateMap<MedicalReport, MedicalReportDTO>()
                .ForMember(dest => dest.ReportType,
                    opt => opt.MapFrom(src => src.ReportType.ToString()));
            CreateMap<CreateMedicalReportDTO, MedicalReport>()
                .ForMember(dest => dest.FileUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CloudinaryPublicId, opt => opt.Ignore())
                .ForMember(dest => dest.ReportType, opt => opt.Ignore()); // Handle manually in service
            CreateMap<UpdateMedicalReportDTO, MedicalReport>()
                .ForMember(dest => dest.ReportType, opt => opt.Ignore()); // Handle manually in service
        }
    }
}
