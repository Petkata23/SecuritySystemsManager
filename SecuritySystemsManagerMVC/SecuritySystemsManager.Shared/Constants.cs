﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared
{
    public static class Constants
    {
        public static readonly string UserNotFound = "User does not exist.\n";
        public const string InvalidPagination = "Invalid pagination parameters.";
        public const string InvalidId = "Invalid ID.";
        public const string InvalidCredentials = "Invalid Credentials.";
        public const string UserAlreadyExists = "User already exists.";
        public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        
        // Date formats
        public const string DateFormat = "dd/MM/yyyy";
        public const string DateTimeFormat = "dd/MM/yyyy HH:mm";
        public const string DateFormatLong = "dd MMMM yyyy";
    }
}
