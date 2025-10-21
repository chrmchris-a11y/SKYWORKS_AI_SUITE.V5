// Phase1 Step2 — Skyworks V5
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok(new { service = "Skyworks API", version = "v1" }));

app.MapGet("/health", () => Results.Ok("OK"));

app.Run();
