namespace SystemGameManager.Handler;

class ErrorHandler
{
    public void HandleError(Exception ex)
    {
        // Log the error or display a message to the user
        Console.WriteLine($"An error occurred: {ex.Message}");
        // You can also log the stack trace or other details if needed
        Console.WriteLine(ex.StackTrace);
    }
}