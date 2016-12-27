using EKIFVK.ChemicalLab.Models;
using Newtonsoft.Json.Linq;

namespace EKIFVK.ChemicalLab.Services
{
    public interface ILoggingService
    {
        void Write(string type, User modifier, string table, int record, JObject data = null);
        void WriteNormal(User modifier, string table, int record, JObject data = null);
        void WriteRejected(User modifier, JObject data = null);
        void WriteWarning(User modifier, string table, int record, JObject data = null);
    }
}
