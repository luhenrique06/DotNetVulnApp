namespace brokenaccesscontrol.Models;

public class UserRequest
{
    public string Name {get; set;}
    public string Login { get; set; }
    public string Password { get; set; }
    public bool? IsAdmin { get; set; }
}

 
