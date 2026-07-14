using System.ComponentModel.DataAnnotations;

namespace PortalVendas.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Informe o código de acesso.")]
    [Display(Name = "Código de acesso")]
    public string CodigoLogin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;
}