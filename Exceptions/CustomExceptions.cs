using System;

namespace StoreManagement.Exceptions
{
    public class InvalidCategoryException : Exception
    {
        public InvalidCategoryException(string message) : base(message) { }
    }

    public class InvalidProductException : Exception
    {
        public InvalidProductException(string message) : base(message) { }
    }

    public class FileFormatException : Exception
    {
        public FileFormatException(string message) : base(message) { }
    }

    public class LoginFailedException : Exception
    {
        public LoginFailedException(string message) : base(message) { }
    }

    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(string message) : base(message) { }
    }

    public class InvalidInputException : Exception
    {
        public InvalidInputException(string message) : base(message) { }
    }
}