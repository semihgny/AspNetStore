using System.ComponentModel.DataAnnotations;

namespace aspnet.Data
{
public class Ogrencis{

    [Key]
    public int id { get; set; }
    public string? name { get; set; }
    public string? surname { get; set; }
    public string? eMail { get; set; }

}
}