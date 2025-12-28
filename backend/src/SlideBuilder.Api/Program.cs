using Microsoft.EntityFrameworkCore;
using SlideBuilder.Core.Configuration;
using SlideBuilder.Api.Hubs;
using SlideBuilder.Api.Logging;
using SlideBuilder.Api.Middleware;
using SlideBuilder.Core.AI;
using SlideBuilder.Core.AI.Parsing;
using SlideBuilder.Core.AI.Prompts;
using SlideBuilder.Core.Artifacts;
using SlideBuilder.Core.Compilation;
using SlideBuilder.Core.Exports;
using SlideBuilder.Core.Jobs;
using SlideBuilder.Core.Persistence;
using SlideBuilder.Core.Services.Outlines;
using SlideBuilder.Core.Services.Projects;
using SlideBuilder.Core.Services.Slides;
using SlideBuilder.Core.Services.Styles;
using SlideBuilder.Core.Storage;
using SlideBuilder.Infrastructure.AI;
using SlideBuilder.Infrastructure.Exports;
using SlideBuilder.Infrastructure.Persistence;
using SlideBuilder.Infrastructure.Persistence.Repositories;
using SlideBuilder.Infrastructure.Storage;
using SlideBuilder.Infrastructure.Compilation;
using SlideBuilder.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<SlideBuilder.Core.Configuration.AppSettings>(builder.Configuration);

builder.Services.AddDbContext<SlideBuilderDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=slidebuilder.db"));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IObjectStorage, OssObjectStorage>();
builder.Services.AddHttpClient<IModelClient, OpenAiCompatibleModelClient>();
builder.Services.AddScoped<IJobRunner, JobRunner>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IOutlineService, OutlineService>();
builder.Services.AddScoped<IStyleBriefService, StyleBriefService>();
builder.Services.AddScoped<ISlideEditService, SlideEditService>();
builder.Services.AddScoped<ISlideAssetInsertService, SlideAssetInsertService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IExportStorageWriter, ExportStorageWriter>();
builder.Services.AddScoped<IAssetStore, AssetStore>();
builder.Services.AddHttpClient<IAssetStore, AssetStore>();

builder.Services.AddScoped<IPresentationArtifactService, PresentationArtifactService>();
builder.Services.AddScoped<IPugCompilationService, PugCompilationService>();

builder.Services.AddScoped<ISlideGenerationPromptBuilder, SlideGenerationPromptBuilder>();
builder.Services.AddScoped<IModelOutputParser, ModelOutputParser>();

builder.Services.AddScoped<IJobStage, SlideBuilder.Core.Jobs.Stages.DraftOutlineStage>();
builder.Services.AddScoped<IJobStage, SlideBuilder.Core.Jobs.Stages.GenerateSlidesStage>();
builder.Services.AddScoped<IJobStage, SlideBuilder.Core.Jobs.Stages.CompilePreviewStage>();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRequestLogging();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Validate Configuration
app.Services.ValidateSettings();

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SlideBuilderDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseRequestLogging();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<JobsHub>("/hubs/jobs");

app.Run();
