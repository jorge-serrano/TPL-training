// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using ConsoleApp;

Console.WriteLine("Hello, World!");
//CancellationWithLinkedTokens();
//CancellationWithWaitHandle();
//WaitInsideTasks();
//BasicHandling();
//AdvancedHandleExceptions();
//SynchronizationWithLock();
//SynchronizationWithInterlocked();
//SynchronizationWithSpinLock();
ReadWriteLockSample();
static void Cancellation()
{
    var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
    var token = cts.Token;
    var task = new Task(
        () =>
        {
            int i = 0;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Timeout o Cancellation was requested");
                    break;
                }

                Console.WriteLine(i++);
            }
        },token
    );
    task.Start();
    Console.ReadKey();
    cts.Cancel();
    Console.WriteLine("End");
}
static void CancellationWithException()
{
    var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
    var token = cts.Token;
    var task = new Task(
        () =>
        {
            int i = 0;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                Console.WriteLine(i++);
            }
        }, token
    );
    task.Start();
    Console.ReadKey();
    cts.Cancel();
    Console.WriteLine("End");
}



static void CancellationWithLinkedTokens()
{
    var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1));
    timeout.Token.Register(() => Console.WriteLine("Timeot..press any key"));
    var userCancellation = new CancellationTokenSource();
    var systemCancellation = new CancellationTokenSource();
    var paranoid =
        CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, systemCancellation.Token,
            userCancellation.Token);
    var task = new Task(
        () =>
        {
            int i = 0;
            while (true)
            {
                paranoid.Token.ThrowIfCancellationRequested();
                Console.WriteLine(i++);
            }
        }, paranoid.Token
    );
    task.Start();
    var key  = Console.ReadKey();
    if (key.Key == ConsoleKey.Escape)
        userCancellation.Cancel();
    else
    {
        systemCancellation.Cancel();
    }
    Console.WriteLine("End");
}
static void CancellationWithWaitHandle()
{
    var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
    var token = cts.Token;
    var task = new Task(
        () =>
        {
            int i = 0;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                Console.WriteLine(i++);
            }
        }, token
    );
    task.Start();
    Task.Factory.StartNew(() =>
    {
        token.WaitHandle.WaitOne();
        Console.WriteLine("Wait handle released");
    });

    Console.ReadKey();
    cts.Cancel();
    Console.WriteLine("End");
}
static void WaitInsideTasks()
{
    var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
    var token = cts.Token;
    var task = new Task(
        () =>
        {
            Console.WriteLine("Press any key before 5 seconds to Disarm");
            bool disarmed = token.WaitHandle.WaitOne(5 * 1000);
            if (disarmed)
                Console.WriteLine("Awesome..you disarmed the Bomb");
            else
            {
                Console.WriteLine("BOOM !!!!!");
            }
        }, token
    );
    task.Start();
    Console.ReadKey();
    cts.Cancel();
    Console.WriteLine("End");
}

static void BasicHandling()
{
    var t = Task.Factory.StartNew(() =>
    {
        throw new InvalidOperationException("Can't do this!") { Source = "t" };
    });

    var t2 = Task.Factory.StartNew(() =>
    {
        var e = new AccessViolationException("Can't access this!");
        e.Source = "t2";
        throw e;
    });

    try
    {
        Task.WaitAll(t, t2);
    }
    catch (AggregateException ae)
    {
        foreach (Exception e in ae.InnerExceptions)
        {
            Console.WriteLine($"Exception {e.GetType()} from {e.Source}.");
        }
    }
}
static void AdvancedHandleExceptions()
{
    

    //      try
    //      {
    //        IterativeHandling();
    //      }
    //      catch (AggregateException ae)
    //      {
    //        Console.WriteLine("Some exceptions we didn't expect:");
    //        foreach (var e in ae.InnerExceptions)
    //          Console.WriteLine($" - {e.GetType()}");
    //      }
    // escalation policy (use Insert Signature CA)
    TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs args) =>
    {
        // this exception got handled
        args.SetObserved();

        var ae = args.Exception as AggregateException;
        ae?.Handle(ex =>
        {
            Console.WriteLine($"Policy handled {ex.GetType()}.");
            return true;
        });
    };
    Task.Factory.StartNew(() =>
    {
        throw new ArgumentOutOfRangeException();
    });
    //IterativeHandling(); // throws


    Console.WriteLine("Main program done, press any key.");
    Console.ReadKey();
}

static void IterativeHandling()
{
    var cts = new CancellationTokenSource();
    var token = cts.Token;
    var t = Task.Factory.StartNew(() =>
    {
        while (true)
        {
            token.ThrowIfCancellationRequested();
            Thread.Sleep(100);
        }
    }, token);

    var t2 = Task.Factory.StartNew(() => throw null);

    cts.Cancel();

    try
    {
        Task.WaitAll(t, t2);
    }
    catch (AggregateException ae)
    {
        // handle exceptions depending on whether they were expected or
        // handles all expected exceptions ('return true'), throws the
        // unhandled ones back as an AggregateException
        ae.Handle(e =>
        {
            if (e is OperationCanceledException)
            {
                Console.WriteLine("Whoops, tasks were canceled.");
                return true; // exception was handled
            }
            else
            {
                Console.WriteLine($"Something went wrong: {e}");
                return false; // exception was NOT handled
            }
        });
    }
    finally
    {
        // what happened to the tasks?
        Console.WriteLine("\tfaulted\tcompleted\tcancelled");
        Console.WriteLine($"t\t{t.IsFaulted}\t{t.IsCompleted}\t{t.IsCanceled}");
        Console.WriteLine($"t1\t{t2.IsFaulted}\t{t2.IsCompleted}\t{t2.IsCanceled}");
    }
}

static void SynchronizationWithLock()
{
    var ba = new BankAccount();
    var tasks = new List<Task>();
    for (int i = 0; i < 10; ++i)
    {
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; ++j)
                ba.DepositV1(100);
        }));
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; ++j)
                ba.WithdrawV1(100);
        }));
    }

    Task.WaitAll(tasks.ToArray());

    Console.WriteLine($"Final balance is {ba.Balance}.");


    Console.WriteLine("All done");
}
static void SynchronizationWithInterlocked()
{
    var ba = new BankAccount();
    var tasks = new List<Task>();
    for (int i = 0; i < 10; ++i)
    {
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; ++j)
                ba.DepositV2(100);
        }));
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; ++j)
                ba.WithdrawV2(100);
        }));
    }

    Task.WaitAll(tasks.ToArray());

    Console.WriteLine($"Final balance is {ba.Balance}.");


    Console.WriteLine("All done");
}
static void SynchronizationWithSpinLock()
{
    var ba = new BankAccount();
    var tasks = new List<Task>();
    SpinLock snLock = new();
    for (int i = 0; i < 10; ++i)
    {
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; ++j)
            {
                var lockTaken = false;
                try
                {
                    snLock.Enter(ref lockTaken);
                    ba.Deposit(100);
                }
                finally
                {
                    if(lockTaken)
                        snLock.Exit();
                    else
                        Console.WriteLine("something went wrong");
                }
                
            }

            
        }));
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; ++j)
            {
                var lockTaken = false;
                try
                {
                    snLock.Enter(ref lockTaken);
                    ba.Withdraw(100);
                }
                finally
                {
                    if (lockTaken)
                        snLock.Exit();
                    else
                        Console.WriteLine("something went wrong");
                }

            }
        }));
    }

    Task.WaitAll(tasks.ToArray());

    Console.WriteLine($"Final balance is {ba.Balance}.");


    Console.WriteLine("All done");
}
static void GlobalMutex()
{
    const string appName = "MyApp";
    Mutex mutex;
    try
    {
        mutex = Mutex.OpenExisting(appName);
        Console.WriteLine($"Sorry, {appName} is already running.");
        return;
    }
    catch (WaitHandleCannotBeOpenedException e)
    {
        Console.WriteLine("We can run the program just fine.");
        // first arg = whether to give current thread initial ownership
        mutex = new Mutex(false, appName);
    }

    Console.ReadKey();
}
static void LocalMutex()
{
    var tasks = new List<Task>();
    var ba = new BankAccount();
    var ba2 = new BankAccount();

    // many synchro types deriving from WaitHandle
    // Mutex = mutual exclusion

    // two types of mutexes
    // this is a _local_ mutex
    Mutex mutex = new Mutex();
    Mutex mutex2 = new Mutex();

    for (int i = 0; i < 10; ++i)
    {
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; ++j)
            {
                bool haveLock = mutex.WaitOne();
                try
                {
                    ba.Deposit(1); // deposit 10000 overall
                }
                finally
                {
                    if (haveLock) mutex.ReleaseMutex();
                }
            }
        }));
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; ++j)
            {
                bool haveLock = mutex2.WaitOne();
                try
                {
                    ba2.Deposit(1); // deposit 10000
                }
                finally
                {
                    if (haveLock) mutex2.ReleaseMutex();
                }
            }
        }));

        // transfer needs to lock both accounts
        tasks.Add(Task.Factory.StartNew(() =>
        {
            for (int j = 0; j < 1000; j++)
            {
                bool haveLock = WaitHandle.WaitAll(new[] { mutex, mutex2 });
                try
                {
                    ba.Transfer(ba2, 1); // transfer 10k from ba to ba2
                }
                finally
                {
                    if (haveLock)
                    {
                        mutex.ReleaseMutex();
                        mutex2.ReleaseMutex();
                    }
                }
            }
        }));
    }

    Task.WaitAll(tasks.ToArray());

    Console.WriteLine($"Final balance is: ba={ba.Balance}, ba2={ba2.Balance}.");
}

static void ReadWriteLockSample()
{
    ReaderWriterLockSlim padlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    int x = 0;

    var tasks = new List<Task>();
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(Task.Factory.StartNew(() =>
        {
            //padlock.EnterReadLock();
            //padlock.EnterReadLock();
            padlock.EnterUpgradeableReadLock();

            if (i % 2 == 0)
            {
                padlock.EnterWriteLock();
                x++;
                padlock.ExitWriteLock();
            }

            // can now read
            Console.WriteLine($"Entered read lock, x = {x}, pausing for 5sec");
            Thread.Sleep(5000);

            //padlock.ExitReadLock();
            //padlock.ExitReadLock();
            padlock.ExitUpgradeableReadLock();

            Console.WriteLine($"Exited read lock, x = {x}.");
        }));
    }

    try
    {
        Task.WaitAll(tasks.ToArray());
    }
    catch (AggregateException ae)
    {
        ae.Handle(e =>
        {
            Console.WriteLine(e);
            return true;
        });
    }

    Random random = new Random();

    while (true)
    {
        Console.ReadKey();
        padlock.EnterWriteLock();
        Console.WriteLine("Write lock acquired");
        int newValue = random.Next(10);
        x = newValue;
        Console.WriteLine($"Set x = {x}");
        padlock.ExitWriteLock();
        Console.WriteLine("Write lock released");
    }
}