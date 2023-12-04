namespace Peasmod4.API.Roles;

public class Enums
{
    public enum Team : int
    {
        Crewmate = 0,
        Impostor = 1,
        Role = 2,
        Alone = 3
    }
    
    public enum Visibility : int
    {
        Everyone = 0,
        Impostor = 1,
        Role = 2,
        NoOne = 3,
        Custom = 4
    }
}