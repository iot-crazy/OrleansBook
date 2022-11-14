namespace OrleansBook.GrainClasses;

public class RobotState
{
    public Queue<string> Instructions { get; set; } = new();
}
