using AiCodeReviewAgent.Application.Reviews;
using AiCodeReviewAgent.Application.Reviews.Rules;
using AiCodeReviewAgent.Application.Repositories;
using AiCodeReviewAgent.Application.Reports;
using AiCodeReviewAgent.Infrastructure.Ai;
using AiCodeReviewAgent.Infrastructure.GitHub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICodeReviewService, CodeReviewService>();
builder.Services.AddScoped<ICodeReviewRule, AvoidConsoleWriteLineRule>();
builder.Services.AddScoped<ICodeReviewRule, AvoidGenericExceptionCatchRule>();
builder.Services.AddScoped<IRepositoryAnalysisService, RepositoryAnalysisService>();
builder.Services.AddScoped<IMarkdownReportService, MarkdownReportService>();
builder.Services.AddScoped<IAiRepositoryAnalysisService, AiRepositoryAnalysisService>();
builder.Services.AddScoped<IAiMarkdownReportService, AiMarkdownReportService>();

builder.Services.AddHttpClient<IAiCodeReviewClient, OpenAiCodeReviewClient>();
builder.Services.AddHttpClient<IGitHubPullRequestClient, GitHubPullRequestClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();