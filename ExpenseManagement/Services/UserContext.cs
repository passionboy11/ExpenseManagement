using System.Security.Claims;

public interface IUserContext
{
    int GetUserId();
    string GetUserEmail();
    bool IsAuthenticated();
}

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated()
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public int GetUserId()
    {
        var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        return idClaim != null ? int.Parse(idClaim.Value) : 0;
    }

    public string GetUserEmail()
        => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
}