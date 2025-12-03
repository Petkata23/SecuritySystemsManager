using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared
{
    public static class Constants
    {
        public static readonly string UserNotFound = "Потребителят не съществува.\n";
        public const string InvalidPagination = "Невалидни параметри за странициране.";
        public const string InvalidId = "Невалидно ID.";
        public const string InvalidCredentials = "Невалидни идентификационни данни.";
        public const string UserAlreadyExists = "Потребителят вече съществува.";
        public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        
        // Date formats
        public const string DateFormat = "dd/MM/yyyy";
        public const string DateTimeFormat = "dd/MM/yyyy HH:mm";
        public const string DateFormatLong = "dd MMMM yyyy";
    }
}
