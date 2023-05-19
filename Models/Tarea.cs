namespace TodoApi.Models;

public class Tarea
{
    public int Id { get; set; }

    public string?Descripcion { get; set; }

     public string? Foto { get; set; }

    public int UserId { get; set; } // Required foreign key property
    public User User { get; set; } = null!; // Required reference navigation to principal

    
}
