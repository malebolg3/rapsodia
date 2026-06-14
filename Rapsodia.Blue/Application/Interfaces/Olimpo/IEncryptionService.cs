namespace Rapsodia.Blue.Application.Interfaces.Olimpo;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}