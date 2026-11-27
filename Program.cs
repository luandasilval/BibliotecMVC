using BibliotecMVC.Data;
using BibliotecMVC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
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
        // options.LoginPath = "/Login/Index"; // Página de login caso n esteja autenticado
        options.AccessDeniedPath = "/Login/AccessDenied"; // Página de acesso negado caso nao tenha permissão

        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Tempo de expiração do cookie

        options.SlidingExpiration = true; // Atualiza o tempo de expiração a cada requisição
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

var app = builder.Build();

// //!Código temporário para atualizar as senhas de teste.
using (var scope = app.Services.CreateScope())
{
    // Obtém o contexto do banco de dados.
    var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Obtém o serviço responsável pela geração do hash.
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

    // Busca os usuários que ainda possuem a senha de teste em texto puro.
    var usuarios = contexto.Usuario
        .Where(u => u.Senha == "admin456")
        .ToList();

    foreach (var usuario in usuarios)
    {
        // Gera o hash da senha do usuário.
        usuario.Senha = passwordHasher.HashPassword(usuario, usuario.Senha);
    }

    // Salva as alterações no banco de dados.
    contexto.SaveChanges();
}

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
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
