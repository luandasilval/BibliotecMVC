using BibliotecMVC.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("ConexaoPadrao")
  )
);

//*Configura a autenticação utilizando cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Configurações do cookie de autenticação
        options.LoginPath = "/Login/Index"; // Página de login caso n esteja autenticado
        options.AccessDeniedPath = "/Login/AccessDenied"; // Página de acesso negado caso nao tenha permissão

        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Tempo de expiração do cookie

        options.SlidingExpiration = true; // Atualiza o tempo de expiração a cada requisição
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

//*A odem é importante. Primeiro o UseAuthentication() para identificar o usuário, depois o UseAuthorization() para verificar se o usuário tem permissão
app.UseAuthentication(); //Identifica o usuário e cria o cookie de autenticação

//Verifica se o usuário está autenticado e autorizado a acessar a rota 
app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
