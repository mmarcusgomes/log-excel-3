using DinkToPdf;
using DinkToPdf.Contracts;
using FeatureLogArquivos;
using FeatureLogArquivos.Interfaces;
using FeatureLogArquivos.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IArquivoService, ArquivoService>();


#region Configura��o DinkToPDF

// Configura��es do DinkToPDF
var ctx = new CustomAssemblyLoadContext();
// Verifica qual a arquiterura para utilizar os arquivos necess�rios
var architectureFolder = (IntPtr.Size == 8) ? "64bit" : "32bit";
// Encontra o caminho onde est�o os arquivos e Carrega os arquivos necess�rios, passadas as configura��es
ctx.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), $"DinkToPDF\\{architectureFolder}\\libwkhtmltox.dll"));
// Configura��o do DinkToPdf
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

#endregion Configura��o DinkToPDF


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
