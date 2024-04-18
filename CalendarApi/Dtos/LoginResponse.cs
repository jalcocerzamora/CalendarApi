namespace CalendarApi.Dtos
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Access { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
