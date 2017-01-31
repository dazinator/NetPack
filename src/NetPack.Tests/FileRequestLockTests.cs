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

            using (var requestLock = FileRequestServices.BlockFilePath("/wwwwroot/somefile.js"))
            {

                // start a backgorund thread that waits for the lock to become free.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

               
                ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
                {
                    waitTask = FileRequestServices.WhenFileNotLocked("/wwwwroot/somefile.js", new TimeSpan(0, 0, 10), CancellationToken.None);
                    manualResetEvent.Set();
                }));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                // simulate some work with the file.
              
                manualResetEvent.WaitOne();

                Assert.Equal(false, waitTask.IsCanceled);
                Assert.Equal(false, waitTask.IsFaulted);
                Assert.Equal(false, waitTask.IsCompleted);

                await Task.Delay(new TimeSpan(0, 0, 4));

                // verify request task still waiting.

            }

            // now that the lock is disposed, the request task should complete
            await waitTask;
            Assert.Equal(true, waitTask.IsCompleted);
            Assert.Equal(false, waitTask.IsFaulted);
            Assert.False(FileRequestServices.HasLock("/wwwwroot/somefile.js"));

        }


    }
}
