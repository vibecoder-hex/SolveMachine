namespace SolveMachine.Models
{
    public abstract class ServiceResult
    {
        public required bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class LoginResult : ServiceResult
    {
        public string? TokenString { get; set; }
    }

    public class RegistrationResult : ServiceResult
    {
        public string? TokenString {  set; get; }
    }
}
