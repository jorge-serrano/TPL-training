// See https://aka.ms/new-console-template for more information
var barrier = new Barrier(3, postPhaseAction: (b) => {
    Console.WriteLine($"Completed {b.CurrentPhaseNumber} step of a total of {b.ParticipantCount}");
});
var cde = new CountdownEvent(3);
var rse = new ManualResetEvent(false);
var semaphore = new SemaphoreSlim(0,1);
Console.WriteLine("Hello, World!");
//ContinuationWhenAll();
//ContinuationAsPromises();
//ContinuationWhenAny();
//ChildTasks();
//TaskBarriers();
//CountdownEvents();
//ManualResetEvents();
Semaphores();

void Semaphores()
{
    var watterTask = Task.Factory.StartNew(BoilWatter);
    var coffeeTask = Task.Factory.StartNew(AddCoffeeToWatter);
    var SugarTask = Task.Factory.StartNew(AddSugarToCoffee);
    var finalResult = Task.Factory.ContinueWhenAll(new[] { watterTask, coffeeTask, SugarTask }
    , tasks => {
        Console.WriteLine($"Executed {tasks.Length} Tasks");
        Console.WriteLine(string.Join('>', tasks.Select(t => t.Status)));
    });
    finalResult.Wait();
}

void ManualResetEvents()
{
    var watterTask = Task.Factory.StartNew(BoilWatter);
    var coffeeTask = Task.Factory.StartNew(AddCoffeeToWatter);
    var SugarTask = Task.Factory.StartNew(AddSugarToCoffee);

    var finalResult = Task.Factory.ContinueWhenAll(new[] { watterTask, coffeeTask, SugarTask }
    , tasks => {
        Console.WriteLine($"Executed {tasks.Length} Tasks");
        Console.WriteLine(string.Join('>', tasks.Select(t => t.Status)));
    });
    finalResult.Wait();
}

void CountdownEvents()
{
    var watterTask = Task.Factory.StartNew(BoilWatter);
    var coffeeTask = Task.Factory.StartNew(AddCoffeeToWatter);
    var SugarTask = Task.Factory.StartNew(AddSugarToCoffee);
    cde.Wait();
    Console.WriteLine("Coffee ready to drink");
}

void TaskBarriers()
{
    var watterTask = Task.Factory.StartNew(BoilWatter);
    var coffeeTask = Task.Factory.StartNew(AddCoffeeToWatter);
    var SugarTask = Task.Factory.StartNew(AddSugarToCoffee);

    var finalResult = Task.Factory.ContinueWhenAll(new[] { watterTask, coffeeTask, SugarTask }
    , tasks => { 
        Console.WriteLine($"Executed {tasks.Length} Tasks");
        Console.WriteLine(string.Join('>', tasks.Select(t => t.Status)));
    });
    finalResult.Wait();
}
void BoilWatter() {
    Console.WriteLine("Boiling Watter ... takes 5 seconds");
    Thread.Sleep(5000);
    //barrier.SignalAndWait();
    //cde.Signal();
    //rse.Set();
    semaphore.Release(1);

}
void AddCoffeeToWatter()
{
    semaphore.Wait();
    //rse.WaitOne();
    Console.WriteLine("Adding Coffee .. takes 1 second");
    Thread.Sleep(30);
    //barrier.SignalAndWait();
    //cde.Signal();
    //rse.Set();
    semaphore.Release(1);
}
void AddSugarToCoffee()
{
    semaphore.Wait();
    //rse.WaitOne();
    Console.WriteLine("Adding sugar to Coffee .. only one instant");
    Thread.Sleep(100);
    //barrier.SignalAndWait();
    //cde.Signal();
    //rse.Set();
    semaphore.Release(1);
}
void ChildTasks()
{
    var parentTask = new Task(() => {
        Console.WriteLine("Parent Task Starting");
        var childTask = new Task(() => {
            Console.WriteLine("Child Task Starting");
            Thread.Sleep(3000);
            Console.WriteLine("Child task Finishing");
        },TaskCreationOptions.AttachedToParent);
        childTask.Start();
    });
    parentTask.Start();
    try
    {
        parentTask.Wait();
    }
    catch (AggregateException ae)
    {
        ae.Handle(e => true);
    }
}

void ContinuationAsPromises()
{
    var task1 = Task.Factory.StartNew<string>(() => "Task1");
    var task2 = Task.Factory.StartNew<string>(function: a => a?.ToString().ToUpper() + " --> Task2", state:task1.Result);
    var task3 = Task.Factory.StartNew<string>(function: a => a?.ToString().ToUpper() + " --> Task3", state:task2.Result);
    var task4 = task2.ContinueWith((t) => task3);
    task4.Wait();
    Console.WriteLine(task4.Result.Result);
}
void ContinuationWhenAll()
{
    var task1 = Task.Factory.StartNew<string>(() => "Task1");
    var task2 = Task.Factory.StartNew<string>(() => "Task2");
    var task3 = Task.Factory.ContinueWhenAll<string>(new[] { task1, task2 }, (tasks) =>
    {
        foreach (var t in tasks)
        {
            Console.WriteLine($"Task {t.Id} executed with result {t.Result}");
        }
    });
    task3.Wait();
}

void ContinuationWhenAny()
{
    var task1 = Task.Factory.StartNew<string>(() => "Task1");
    var task2 = Task.Factory.StartNew<string>(() => "Task2");
    var task3 = Task.Factory.ContinueWhenAny<string>(new[] { task1, task2 }, (t) =>
    {
        if (t.IsCompleted)
            Console.WriteLine($"Task {t.Id} executed with result {t.Result}");
        
    });
    task3.Wait();
}