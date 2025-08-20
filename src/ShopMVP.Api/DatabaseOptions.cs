using System.ComponentModel.DataAnnotations;

namespace ShopMVP.Api.DatabaseOptions;

public class DatabaseOptions
{
    public const string Option = "ConnectionStrings";
    [Required]
    public string Default { get; set; } = string.Empty;

}
