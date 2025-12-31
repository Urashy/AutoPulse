namespace BlazorAutoPulse.Model;

public class A2fResponse
{
    public string Message { get; set; }
    public bool RequiresA2f { get; set; }
    public bool MustReactivate { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
}