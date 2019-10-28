using System;
using System.Threading.Tasks;

namespace async.Async
{
    // TODO -- switch to the await versions of the same
    
    public class WaitingForTasks
    {
        public Task WaitingForThingsToFinish(Task operation1, Task operation2, Task operation3)
        {
            // Programmatically wait for a task to finish:
            // THIS IS PRONE TO DEADLOCKS AND SHOULD NOT BE DONE IN 
            // PRODUCTION CODE
            operation1.Wait();
            // or
            operation1.Wait(TimeSpan.FromSeconds(30));
            
            
            
            // Proceed as soon as any of the operations complete
            Task.WaitAny(operation1, operation2, operation3);


            // Wait for all of these to finish
            Task.WaitAll(operation1, operation2, operation3);
            
            
            return Task.CompletedTask;
        }
    }
}