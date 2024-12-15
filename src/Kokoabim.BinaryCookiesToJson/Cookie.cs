namespace BinaryCookiesToJson;

internal class Cookie
{
    #region properties
    public DateTime Created { get; set; }
    public string Domain { get; set; } = default!;
    public DateTime Expires { get; set; }
    public bool HttpOnly { get; set; }
    public string Name { get; set; } = default!;
    public string Path { get; set; } = default!;
    public bool Secure { get; set; }
    public string Value { get; set; } = default!;
    #endregion 
}