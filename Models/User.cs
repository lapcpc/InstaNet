namespace TodoApi.Models;

public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string? ProfilePic { get; set; }

    public string? Descripcion { get; set;}

     public ICollection<Tarea> Tareas { get; } = new List<Tarea>();
}