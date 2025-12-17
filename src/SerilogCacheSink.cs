using Serilog.Events;
using Serilog.Formatting.Display;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Serilog.Sinks.Cache.Abstract;
using Soenneker.Serilog.Sinks.Cache.Dtos;
using Soenneker.Utils.ReusableStringWriter;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Soenneker.Atomics.Bools;

namespace Soenneker.Serilog.Sinks.Cache;

///<inheritdoc cref="ISerilogCacheSink"/>
public sealed class SerilogCacheSink : ISerilogCacheSink
{
    private const string _defaultTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    private readonly Channel<Msg> _ch = Channel.CreateUnbounded<Msg>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false
    });

    private readonly Task _readerTask;
    private readonly MessageTemplateTextFormatter _fmt;
    private readonly ReusableStringWriter _sw = new();

    // Queue-backed cache (reader-only)
    private readonly Queue<Entry> _q;
    private readonly int? _capacity; // null => unbounded
    private readonly long? _byteBudget; // null => no byte limit
    private long _qBytes;

    // Lifecycle & toggle
    private readonly AtomicBool _disposed = new();
    private readonly AtomicBool _enabled = new(true);

    public int? Capacity => _capacity;
    public long? ByteBudget => _byteBudget;
    public bool IsEnabled => _enabled.Value;
    private bool IsDisposed => _disposed.Value;

    public SerilogCacheSink(int? capacity = null, long? byteBudget = null, string outputTemplate = _defaultTemplate, IFormatProvider? formatProvider = null)
    {
        if (capacity is < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (byteBudget is < 0)
            throw new ArgumentOutOfRangeException(nameof(byteBudget));

        _capacity = capacity;
        _byteBudget = byteBudget;

        _q = new Queue<Entry>(capacity.GetValueOrDefault() > 0 ? capacity!.Value : 4);
        _fmt = new MessageTemplateTextFormatter(outputTemplate, formatProvider);

        _readerTask = Task.Run(ReadLoop);
    }

    public void Emit(LogEvent logEvent)
    {
        if (!_enabled.Value || IsDisposed)
            return;

        _ch.Writer.TryWrite(new LogEvt(logEvent));
    }

    public Task<List<string>> Snapshot()
    {
        var tcs = new TaskCompletionSource<List<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (IsDisposed)
        {
            tcs.SetResult([]);
            return tcs.Task;
        }

        _ch.Writer.TryWrite(new SnapshotReq(tcs));
        return tcs.Task;
    }

    public Task<List<string>> Drain()
    {
        var tcs = new TaskCompletionSource<List<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (IsDisposed)
        {
            tcs.SetResult([]);
            return tcs.Task;
        }

        _ch.Writer.TryWrite(new DrainReq(tcs));
        return tcs.Task;
    }

    public Task Clear()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (IsDisposed)
        {
            tcs.SetResult(true);
            return tcs.Task;
        }

        _ch.Writer.TryWrite(new ClearReq(tcs));
        return tcs.Task;
    }

    public Task Enable()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (IsDisposed)
        {
            tcs.SetResult(false);
            return tcs.Task;
        }

        _ch.Writer.TryWrite(new EnableReq(tcs));
        return tcs.Task;
    }

    public Task Disable()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (IsDisposed)
        {
            tcs.SetResult(false);
            return tcs.Task;
        }

        _ch.Writer.TryWrite(new DisableReq(tcs));
        return tcs.Task;
    }

    private async Task ReadLoop()
    {
        await foreach (Msg msg in _ch.Reader.ReadAllAsync()
                           .ConfigureAwait(false))
        {
            switch (msg)
            {
                case LogEvt(var evt):
                {
                    _sw.Reset();
                    _fmt.Format(evt, _sw);
                    string line = _sw.Finish();
                    Append(line);
                    break;
                }

                case SnapshotReq(var tcs):
                    tcs.TrySetResult(GetSnapshot());
                    break;

                case DrainReq(var tcs):
                    tcs.TrySetResult(DrainInternal());
                    break;

                case ClearReq(var tcs):
                    ClearInternal();
                    tcs.TrySetResult(true);
                    break;

                case EnableReq(var tcs):
                    _enabled.Value = true;
                    tcs.TrySetResult(true);
                    break;

                case DisableReq(var tcs):
                    _enabled.Value = false;
                    tcs.TrySetResult(true);
                    break;
            }
        }
    }

    // ---- Reader-owned helpers ----

    private void Append(string line)
    {
        var e = new Entry(line);
        _q.Enqueue(e);
        _qBytes += e.Bytes;

        if (_capacity is > 0)
        {
            while (_q.Count > _capacity.Value)
                EvictOne();
        }

        if (_byteBudget is > 0)
        {
            while (_qBytes > _byteBudget.Value && _q.Count > 0)
                EvictOne();
        }
    }

    private void EvictOne()
    {
        Entry old = _q.Dequeue();
        _qBytes -= old.Bytes;
    }

    private List<string> GetSnapshot()
    {
        var res = new List<string>(_q.Count);
        foreach (Entry e in _q)
            res.Add(e.Line);
        return res;
    }

    private List<string> DrainInternal()
    {
        var res = new List<string>(_q.Count);
        while (_q.Count > 0)
        {
            Entry e = _q.Dequeue();
            res.Add(e.Line);
        }

        _qBytes = 0;
        return res;
    }

    private void ClearInternal()
    {
        _q.Clear();
        _qBytes = 0;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed.TrySetTrue())
            return;

        _ch.Writer.TryComplete();
        await _readerTask.NoSync();
        await _sw.DisposeAsync()
            .NoSync();
    }

    public void Dispose()
    {
        if (!_disposed.TrySetTrue())
            return;

        _ch.Writer.TryComplete();

        try
        {
            _readerTask.GetAwaiter()
                .GetResult();
        }
        catch
        {
            /* swallow */
        }

        _sw.DisposeAsync()
            .AsTask()
            .GetAwaiter()
            .GetResult();
    }
}