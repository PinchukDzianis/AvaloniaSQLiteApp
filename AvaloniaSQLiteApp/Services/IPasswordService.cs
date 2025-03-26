using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaSQLiteApp.Services
{
    public interface IPasswordService
    {
        bool VerifyPassword(string enteredPassword, string storedHash, string saltBase64);
        (string Salt, string Hash) HashPassword(string password);
    }
}
