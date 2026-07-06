using System.Text.Json.Serialization;
using ControleGastos.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Enums serializados como texto no JSON ("Despesa"/"Receita" em vez de 0/1).
builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Banco de dados: SQLite via Entity Framework Core.
// O SQLite grava tudo em um arquivo local (controle_gastos.db), o que garante
// que os dados continuam salvos depois que a aplicação é fechada.
builder.Services.AddDbContext<AppDbContext>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("ControleGastos")));

// CORS: autoriza o front-end local (http://localhost:5173) a chamar esta API.
const string PoliticaCorsFrontend = "PermitirFrontend";
builder.Services.AddCors(opcoes =>
    opcoes.AddPolicy(PoliticaCorsFrontend, politica =>
        politica.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()));

// Swagger: página interativa de documentação/testes em /swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Cria o banco e as tabelas na primeira execução, caso ainda não existam.
// Para um projeto deste porte, EnsureCreated simplifica o setup (basta rodar);
// em um sistema que evolui continuamente, o ideal seria usar Migrations.
using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(PoliticaCorsFrontend);

app.MapControllers();

app.Run();
