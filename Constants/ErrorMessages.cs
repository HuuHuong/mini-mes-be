namespace mini_mes_be.Constants;

public static class ErrorMessages
{
    public const string UnexpectedError = "An unexpected error occurred.";

    public static class Auth
    {
        public const string InvalidCredentials   = "Invalid credentials.";
        public const string EmailNotRegistered   = "Email is not registered.";
        public const string AccountInactive      = "Account is inactive.";
        public const string IncorrectPassword    = "Incorrect password.";
        public const string EmailAlreadyTaken    = "Email is already taken.";
        public const string UsernameAlreadyTaken = "Username is already taken.";

        public const string InvalidRefreshToken  = "Invalid or expired refresh token.";
        public const string RevokedRefreshToken  = "Invalid or already revoked refresh token.";

        public const string RegistrationSuccess  = "Registration successful";
        public const string LoginSuccess         = "Login successful";
        public const string TokenRefreshed       = "Token refreshed";
    }

    public static class Validation
    {
        public const string UsernameRequired      = "Username is required.";
        public const string UsernameMinLength      = "Username must be at least 3 characters.";
        public const string UsernameMaxLength      = "Username must not exceed 100 characters.";

        public const string EmailRequired          = "Email is required.";
        public const string EmailInvalid           = "A valid email address is required.";

        public const string PasswordRequired       = "Password is required.";
        public const string PasswordMinLength      = "Password must be at least 8 characters.";
        public const string PasswordMaxLength      = "Password must not exceed 128 characters.";
        public const string PasswordNeedsLowercase = "Password must contain at least one lowercase letter.";
        public const string PasswordNeedsUppercase = "Password must contain at least one uppercase letter.";
        public const string PasswordNeedsDigit     = "Password must contain at least one digit.";
        public const string PasswordNeedsSpecial   = "Password must contain at least one special character.";
    }
}
