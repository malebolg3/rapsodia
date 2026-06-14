namespace Rapsodia.Blue.Application.Interfaces.Olimpo;

public interface ITotpService
{
    string GenerateCode(string secret, string algorithm, int digits, int period);
}