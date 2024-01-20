using System.Text.Json.Serialization;

namespace tsuyu.Models; 

public class User {
    public uint Id { get; set; }
    public string Username { get; set; }
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
    /// <summary>
    /// Permanent token that can be used to authorize with a Bearer header.
    /// </summary>
    public string? ApiToken { get; set; }
    public DateTime LastUpdate { get; set; }
    public DateTime CreatedAt { get; set;}

    /// <summary>
    /// Hashed Password using Bcrypt.
    /// </summary>
    [JsonIgnore]
    public string HashedPassword { get; set; }
}