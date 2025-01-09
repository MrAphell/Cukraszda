using Grpc.Core;
using Cukraszda_Server.Protos;
using Cukraszda_Server.Services;

public class AuthServiceImpl : AuthService.AuthServiceBase
{
    private readonly DataService dataService;

    public AuthServiceImpl(DataService dataService)
    {
        this.dataService = dataService;
    }

    public override async Task<AuthResponse> Authenticate(AuthRequest request, ServerCallContext context)
    {
        var user = dataService.GetUserByUsername(request.Username);

        if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new AuthResponse
            {
                Success = true,
                Token = GenerateToken(user)
            };
        }

        return new AuthResponse { Success = false };
    }

    private string GenerateToken(UserData user)
    {
        return "valid_token";
    }
}
