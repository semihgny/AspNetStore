using System.ComponentModel.DataAnnotations;

namespace aspnet.Data
{
public class Kurskayit{

    [Key]
    public int kayitId { get; set; }
    public string? courseId { get; set; }
    public string? ogrenciId { get; set; }
    public DateTime kayitTarihi { get; set; }
}
}