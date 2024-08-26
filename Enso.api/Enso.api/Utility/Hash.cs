using CryptoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enso.api.Utility
{
    public class Hash
    {
        // Encriptar password
        public string HashPassword(string password)
        {
            return Crypto.HashPassword(password);
        }

        // Validar password al inicio de sesión
        // SIEMPRE espera una cadena hash y una cadena password: (hash, password)
        public bool VerifyPassword(string hash, string password)
        {
            return Crypto.VerifyHashedPassword(hash, password);
        }
    }
}
