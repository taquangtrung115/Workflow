using Microsoft.EntityFrameworkCore;
using Workflow.Data;
using Workflow.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework Core với SQL Server
builder.Services.AddDbContext<WorkflowDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Register application services
builder.Services.AddScoped<IFileTypeService, FileTypeService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// Register approver strategies
builder.Services.AddSingleton<Workflow.Services.Strategies.IApproverStrategy, Workflow.Services.Strategies.UsersApproverStrategy>();
builder.Services.AddSingleton<Workflow.Services.Strategies.IApproverStrategy, Workflow.Services.Strategies.DepartmentApproverStrategy>();
builder.Services.AddSingleton<Workflow.Services.Strategies.ApproverStrategyFactory>();

// Register validators and pipeline
builder.Services.AddScoped<Workflow.Services.Validators.ValidationPipeline>(sp =>
{
    var fileTypeService = sp.GetRequiredService<IFileTypeService>();
    var permissionService = sp.GetRequiredService<IPermissionService>();
    var strategyFactory = sp.GetRequiredService<Workflow.Services.Strategies.ApproverStrategyFactory>();

    var pipeline = new Workflow.Services.Validators.ValidationPipeline();
    pipeline.AddValidator(new Workflow.Services.Validators.FileTypeValidator());
    pipeline.AddValidator(new Workflow.Services.Validators.UserFileTypePermissionValidator(fileTypeService, permissionService));
    pipeline.AddValidator(new Workflow.Services.Validators.ApproverScopeValidator(strategyFactory));
    
    return pipeline;
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Workflow API",
        Version = "v1",
        Description = "API cho hệ thống Signature Workflow - Quản lý quy trình duyệt tài liệu"
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<WorkflowDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Serve static files from uploads directory
app.UseStaticFiles();

app.Run();
