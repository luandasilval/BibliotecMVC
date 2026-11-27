
using BibliotecMVC.Data;
using BibliotecMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecMVC.Controllers;

//permite o acesso a apenas usuarios autenticados (administradores e alunos)
[Authorize(Roles = "Administrador, Aluno")]
public class LivroController : Controller
{
    private readonly AppDbContext _context;

    public LivroController(AppDbContext context)
    {
        _context = context;
    }
    //lista os livros cadastrados no banco de dados
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var livros = await _context.Livro
        .Include(l => l.IdAutorNavigation)
        .Include(l => l.IdStatusNavigation)
        .Include(l => l.IdCategoria)
        .OrderBy(l => l.Titulo)
        .ToListAsync();
        
        if(User.IsInRole("Administrador"))
        {
            //retorna a view de listagem de livros para o administrador
            return View("ListagemAdministrador", livros);
        }
        //retorna a view de listagem de livros para o aluno
        return View("ListagemAluno", livros);
    }
}