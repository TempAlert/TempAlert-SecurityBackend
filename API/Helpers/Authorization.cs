namespace API.Helpers;
public class Authorization
{
    public enum Rols
    {
        Administrator,
        Employed
    }

    public const Rols default_rol = Rols.Employed;
}
