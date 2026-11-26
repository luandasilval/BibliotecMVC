using System.Security.Claims;
using BibliotecMVC.Data;
using BibliotecMVC.Models;
using BibliotecMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecMVC.Controllers;

public class LoginController : Controller
{
    //*recebendo dependencias registradas na program
    //permite consultar e manipular os dados do banco
    private readonly AppDbContext _context;

    //permte gerar e validar o hash das senhas
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public LoginController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }


    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    //Metodo que recebe os dados do formulário
    public async Task<IActionResult> Index(LoginViewModel model)
    {


        // Verifica se os dados do formulário são válidos
        if (!ModelState.IsValid)
        {
            return View(model);
        }


        //*pega o usuario do banco de dados com base no email fornecido, 
        //*faça isso sem travar o sistema(por isso o uso do await)
        Usuario usuario = await _context.Usuario
         .FirstOrDefaultAsync(u => u.Email == model.Email && u.Excluido == false);

        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
            return View(model);
        }

        // Verifica se a senha fornecida corresponde ao hash armazenado no banco de dados
        var resultadoSenha = _passwordHasher.VerifyHashedPassword(
            usuario, //objeto que representa o usuário encontrado no banco de dados
            usuario.Senha, // hash da senha armazenado no banco de dados
            model.Senha); // senha digitada pelo usuário no formulário de login

        if (resultadoSenha == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
            return View(model);
        }


        if (resultadoSenha == PasswordVerificationResult.Success)
        {
            //GERA UM NOVO HASH DA SENHA UTILIZANDO AS CONFIGURAÇÕES DE SEGURANÇA ATUAIS E ATUALIZA O HASH NO BANCO DE DADOS
            usuario.Senha = _passwordHasher.HashPassword(usuario, model.Senha);
            await _context.SaveChangesAsync();
        }
        //verifica se o usuário é administrador
        //verifica dentro da tabela de administradores se existe algum registro com o id do usuário
        //retorna true se existir 
        bool administrador = await _context.Administrador
        .AnyAsync(a => a.IdAdministrador == usuario.IdUsuario);

        //verifica se o usuario esta cadastrado como aluno
        bool aluno = await _context.Aluno.AnyAsync(a => a.IdAluno == usuario.IdUsuario);

        //coloca o tipo do perfil do usuario em uma variavel tipoUsuario
        string? tipoUsuario = administrador ? "Administrador" : aluno ? "Aluno" : null;

        //cria uma lista de claims com as informações do usuario
        //claims -> informações do usuario que permitem identificar e controlar o acesso
        var claims = new List<Claim>
 {
    //armazena o identificador do usuario (id)
    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),

    //armazena o nome do usuario
    new Claim(ClaimTypes.Name, usuario.Nome),

    //armazena o email do usuario
    new Claim(ClaimTypes.Email, usuario.Email),

    //armazena o tipo do usuario para controle de acesso
    new Claim(ClaimTypes.Role, tipoUsuario ?? string.Empty)
};

        //cria a identidad do usuario utilizando as claims 
        //claimsIdentity -> carteirinha de identificação do usuario dentro do sistema 
        // -> cookies -> informa que estamos utilizando o tipo  para autenticação por cookies
        var identidade = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
            );

        //cria o principal do usuario utilizando a identidade
        //classe dos asp.net para representar o usuario
        var principal = new ClaimsPrincipal(identidade);

        //realiza a autenticação e gera o cookie
        await HttpContext.SignInAsync(
         CookieAuthenticationDefaults.AuthenticationScheme,
         principal
        );

        return RedirectToAction("Index", "Home");
    }
}

