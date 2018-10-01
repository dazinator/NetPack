using NetPack.FileLocking;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NetPack.Tests
{
    public class FileRequestLockTests
    {

        [Fact]
        public async Task Can_Create_A_Request_Lock_For_File()
        {

            //  var fileProvider = new InMemoryFileProvider();
            //  fileProvider.Directory.AddFile("wwwroot", new StringFileInfo("hi", "somefile.js"));


            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            Task waitTask = null;

            using (var requestLock = new FileLocker().AddBlock("/wwwwroot/somefile.js"))
            {

                // start a backgorund thread that waits for the lock to become free.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

               
                ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
                {
                    waitTask = FileLocks.WaitIfLockedAsync("/wwwwroot/somefile.js", new TimeSpan(0, 0, 10), CancellationToken.None);
                    manualResetEvent.Set();
                }));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                // simulate some work with the file.
              
                manualResetEvent.WaitOne();

                Assert.False(waitTask.IsCanceled);
                Assert.False(waitTask.IsFaulted);
                Assert.False(waitTask.IsCompleted);

                await Task.Delay(new TimeSpan(0, 0, 4));

                // verify request task still waiting.
                Assert.False(waitTask.IsCanceled);
                Assert.False(waitTask.IsFaulted);
                Assert.False(waitTask.IsCompleted);
            }

            // now that the lock is disposed, the request task should complete
            await waitTask;
            Assert.True(waitTask.IsCompleted);
            Assert.False(waitTask.IsFaulted);
           // Assert.False(FileRequestServices.HasLock("/wwwwroot/somefile.js"));

        }


    }
}
