using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Patient_mgt.Application;
using Patient_mgt.Data;
using Patient_mgt.Infrastructure;
using Patient_mgt.Infrastructure.RAG;
using Patient_mgt.Mappings;
using Patient_Management_Module.Middleware;



AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);  //config-appsettings



builder.Services.AddControllers();  //reg MVC controller to handle HTTP req


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<PatientContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("dbConn")));





builder.Services.AddScoped<IPatient, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IUser, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IEMR, EMRRepository>();
builder.Services.AddScoped<IEMRService, EMRService>();
builder.Services.AddScoped<IInsurance, InsuranceRepository>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<IMedicalReportRepository, MedicalReportRepository>();
builder.Services.AddScoped<IMedicalReportService, MedicalReportService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IIcdService, IcdService>();
builder.Services.AddHttpClient<IRagService, RagService>();
builder.Services.AddScoped<ChatService>();

builder.Services.AddScoped<IQueryRouter, QueryRouter>();
builder.Services.AddScoped<IPatientDataService, PatientDataService>();


builder.Services.AddSingleton<IMapper>(sp =>
{
    var config = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    }, sp.GetRequiredService<ILoggerFactory>());

    return config.CreateMapper();
});



//jwt authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(op =>
    {
        op.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Key"])),
            ValidateIssuer = false,
            ValidateAudience = false,
        };

    });




//authorize btn in swagger
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
{
new OpenApiSecurityScheme
{
Reference = new OpenApiReference
{
Type = ReferenceType.SecurityScheme,
Id = "Bearer"
}
},
Array.Empty<string>()
}
});
});



//detected cycle error - while .include of other model
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});


// Cross-Origin Resource Sharing 
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAngular", pol => pol.WithOrigins("http://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    );

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//middleware pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
