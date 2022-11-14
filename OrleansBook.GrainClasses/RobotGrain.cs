using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.BroadcastChannel;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;
using OrleansBook.GrainInterfaces;
using System.Runtime.CompilerServices;

namespace OrleansBook.GrainClasses;


[ImplicitChannelSubscription("robotChannel")]
[Reentrant]
public sealed class RobotGrain : Grain, IRobotGrain, IOnBroadcastChannelSubscribed
{
    // IPersistentState<RobotState> State;
    ITransactionalState<RobotState> State;
    ILogger<RobotGrain> Logger;
    IBroadcastChannelProvider BroadcastChannel;
    ChannelId ChannelId;
    string? GrainKey;
    IBroadcastChannelWriter<string> BroadcastChannelWriter;


    public RobotGrain(ILogger<RobotGrain> logger,
        [TransactionalState("robotState", "robotStateStore")] ITransactionalState<RobotState> state
        )
    // [PersistentState("robotState", "orleansbookstore")] IPersistentState<RobotState> state)
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

        // State.State.Instructions.Enqueue(instruction);
        // await State.WriteStateAsync();

        await State.PerformUpdate(state => state.Instructions.Enqueue(instruction));

        await BroadcastChannelWriter.Publish(instruction);
    }

    public async Task<int> GetInstructionCount()
    {
        //return Task.FromResult(State.Instructions.Count);

        return await State.PerformRead(state => state.Instructions.Count);
    }

    public async Task<string> GetNextInstruction()
    {
        var key = this.GetPrimaryKeyString();
        string instruction = string.Empty;
        await State.PerformUpdate(state =>
        {
            if (State.PerformRead(state => state.Instructions.Count).Result == 0) return;
            instruction = State.PerformRead(state => state.Instructions.Dequeue()).Result;
        });

        return instruction;

        /* if (State.Instructions.Count == 0)
         {
             return string.Empty;
         }
         var instruction = State.State.Instructions.Dequeue();
         await State.WriteStateAsync();
         return instruction;*/



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