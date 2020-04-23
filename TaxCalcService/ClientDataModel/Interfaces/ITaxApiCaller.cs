namespace Models.Interfaces
{
    public interface ITaxApiCaller
    {
        string Identifier { get; set; }

        string AuthenticationToken { get; set; }

        string Version { get; set; }
    }
}
