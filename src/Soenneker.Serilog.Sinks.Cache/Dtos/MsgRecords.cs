using Serilog.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soenneker.Serilog.Sinks.Cache.Dtos
{
    // Messages handled by the single reader
    internal abstract record Msg;

    internal sealed record LogEvt(LogEvent Event) : Msg;

    internal sealed record SnapshotReq(TaskCompletionSource<List<string>> Tcs) : Msg;

    internal sealed record DrainReq(TaskCompletionSource<List<string>> Tcs) : Msg;

    internal sealed record ClearReq(TaskCompletionSource<bool> Tcs) : Msg;

    internal sealed record EnableReq(TaskCompletionSource<bool> Tcs) : Msg;

    internal sealed record DisableReq(TaskCompletionSource<bool> Tcs) : Msg;
}