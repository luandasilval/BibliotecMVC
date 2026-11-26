
using System.ComponentModel.DataAnnotations;

namespace BibliotecMVC.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Informe o Email.")]
        [EmailAddress(ErrorMessage = "Informe um Email válido.")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe a Senha.")]
        public string Senha { get; set; } = string.Empty;
    }
}