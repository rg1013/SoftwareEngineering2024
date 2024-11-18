namespace Updater;

public class BinarySemaphore
{
    private SemaphoreSlim _semaphore;

    // Constructor to initialize the _semaphore with an initial count of 1 (binary state)
    public BinarySemaphore()
    {
        _semaphore = new SemaphoreSlim(1, 1);  // Initial count of 1, maximum count of 1
    }

    // Wait method that blocks until the _semaphore is available
    public void Wait()
    {
        _semaphore.Wait();  // Blocks the current thread until it can enter
    }

    // Signal method that releases the _semaphore, allowing a waiting thread to proceed
    public void Signal()
    {
        _semaphore.Release();  // Releases the _semaphore, allowing one thread to enter
    }
}
