using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace NetPack.File
{
    public class StringFileInfo : IFileInfo
    {
        private readonly string _contents;
        private Lazy<long> _lazyLength;

        private static Encoding _Encoding = Encoding.UTF8;
        
        public StringFileInfo(string contents, string name)
        {
            _contents = contents;
            LastModified = DateTimeOffset.UtcNow;
            IsDirectory = false;
            Name = name;
            PhysicalPath = null;
            Exists = true;

            _lazyLength = new Lazy<long>(() =>
            {
                return _Encoding.GetByteCount(_contents);
            });


        }

        public Stream CreateReadStream()
        {
            return new MemoryStream(_Encoding.GetBytes(_contents ?? ""));
        }

        public bool Exists { get; }
        public long Length { get { return _lazyLength.Value; } }
        public string PhysicalPath { get; }
        public string Name { get; }
        public DateTimeOffset LastModified { get; }
        public bool IsDirectory { get; }


    }

    public class InMemoryChangeToken : IChangeToken
    {

       // public StringFileInfo File { get; set; }

        public InMemoryChangeToken()
        {
          //  File = file;
        }

        private readonly ConcurrentBag<Tuple<Action<object>, object, IDisposable>> _callbacks = new ConcurrentBag<Tuple<Action<object>, object, IDisposable>>();

        public bool ActiveChangeCallbacks { get; set; }

        public bool HasChanged { get; set; }

        public ConcurrentBag<Tuple<Action<object>, object, IDisposable>> Callbacks
        {
            get
            {
                return _callbacks;
            }
        }

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            var disposable = EmptyDisposable.Instance;
            _callbacks.Add(Tuple.Create(callback, state, (IDisposable)disposable));
            return disposable;
        }

        public void RaiseCallback(object newItem)
        {
            foreach (var callback in _callbacks)
            {
                callback.Item1(newItem);
            }
        }
    }

    internal class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance { get; } = new EmptyDisposable();

        private EmptyDisposable()
        {
        }

        public void Dispose()
        {
        }
    }
}