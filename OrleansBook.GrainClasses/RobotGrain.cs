using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClasses;

public class RobotGrain : IRobotGrain
{
    IPersistentState<RobotState> State;
    ILogger<RobotGrain> Logger;


    public RobotGrain(ILogger<RobotGrain> logger,
        [PersistentState("robotState", "robot-StateStore")] IPersistentState<RobotState> state)
    {
        Logger = logger;
        State = state;
    }

    public async Task AddInstruction(string instruction)
    {
        var key = this.GetPrimaryKeyString();
        Logger.LogWarning($"{key} adding instruction {instruction}");

        State.State.Instructions.Enqueue(instruction);
        await State.WriteStateAsync();
    }

    public Task<int> GetInstructionCount()
    {
        return Task.FromResult(State.State.Instructions.Count);
    }

    public async Task<string> GetNextInstruction()
    {
        if (State.State.Instructions.Count == 0)
        {
            return string.Empty;
        }
        var instruction = State.State.Instructions.Dequeue();
        await State.WriteStateAsync();
        return instruction;
    }
}