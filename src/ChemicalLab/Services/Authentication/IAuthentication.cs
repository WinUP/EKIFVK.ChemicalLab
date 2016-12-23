using EKIFVK.ChemicalLab.Models;
using Microsoft.AspNetCore.Http;

namespace EKIFVK.ChemicalLab.Services.Authentication
{
    public interface IAuthentication
    {
        string FindToken(IHeaderDictionary header);
        VerifyResult Verify(string token, string[] permissions, string ip = "");
        VerifyResult Verify(string token, string permission, string ip = "");
        VerifyResult Verify(User user, string[] permissions, string ip = "");
        VerifyResult Verify(User user, string permission, string ip = "");
        void UpdateAccessTime(string token);
        void UpdateAccessTime(User user);
        string ToString(VerifyResult result);
    }
}