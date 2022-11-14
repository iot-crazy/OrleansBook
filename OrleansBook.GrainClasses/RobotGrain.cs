using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.BroadcastChannel;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansBook.GrainInterfaces;
using System.Runtime.CompilerServices;

namespace OrleansBook.GrainClasses;


[ImplicitChannelSubscription("robotChannel")]
[Reentrant]
public sealed class RobotGrain : Grain, IRobotGrain, IOnBroadcastChannelSubscribed
{
    IPersistentState<RobotState> State;
    ILogger<RobotGrain> Logger;
    IBroadcastChannelProvider BroadcastChannel;
    ChannelId ChannelId;
    string? GrainKey;
    IBroadcastChannelWriter<string> BroadcastChannelWriter;


    public RobotGrain(ILogger<RobotGrain> logger,
        [PersistentState("robotState", "orleansbookstore")] IPersistentState<RobotState> state)
    {
        Logger = logger;
        State = state;
        BroadcastChannel = this.ServiceProvider.GetRequiredServiceByName<IBroadcastChannelProvider>("robotChannel");

        GrainKey = Guid.NewGuid().ToString("N");
        ChannelId = ChannelId.Create("robotChannel", GrainKey);
        BroadcastChannelWriter = BroadcastChannel.GetChannelWriter<string>(ChannelId);
        
    }

    public async Task AddInstruction(string instruction)
    {
        var key = this.GetPrimaryKeyString();
        Logger.LogWarning($"{key} adding instruction {instruction}");

        State.State.Instructions.Enqueue(instruction);
        await State.WriteStateAsync();

        await BroadcastChannelWriter.Publish(instruction);
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

    public Task OnSubscribed(IBroadcastChannelSubscription streamSubscription)
    {
        streamSubscription.Attach<string>(
         item => OnPublished(streamSubscription.ChannelId, item),
         ex => OnError(streamSubscription.ChannelId, ex));

        return Task.CompletedTask;

        // Called when an item is published to the channel
        static Task OnPublished(ChannelId id, string item)
        {
            // Do something
            Console.WriteLine($"new message received: {item}");
            return Task.CompletedTask;
        }

        // Called when an error occurs
        static Task OnError(ChannelId id, Exception ex)
        {
            // Do something
            return Task.CompletedTask;
        }
    }
}