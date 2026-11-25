using System;
using System.Collections.Generic;

namespace BibliotecMVC.Models;

public partial class Autor
{
    public int IdAutor { get; set; }

    public string Nome { get; set; } = null!;

    public string? Nacionalidade { get; set; }

    public virtual ICollection<Livro> Livro { get; set; } = new List<Livro>();
}
