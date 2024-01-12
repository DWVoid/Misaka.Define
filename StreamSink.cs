using System.Runtime.CompilerServices;

namespace Misaka.Define;

/// <summary>
/// Interface for implementing and assembling bulk processing pipelines of stream of unmanaged datatype 
/// </summary>
/// <typeparam name="T"> Unmanaged datatype to be processed </typeparam>
public interface IStreamSink<T> where T : unmanaged
{
    /// <summary>
    /// Try to supply data to the processing stage.
    /// Supplied data should be treated as it is immediately available to the stage.
    /// If caching or batching is to be implemented, flush call could be used for processing.
    /// Implementers are expected to process as much data as possible during this call. <br/>
    /// No exceptions other than fatal ones and OutOfMemory is allowed to exit this call.
    /// </summary>
    /// <param name="data"> The supplied data </param>
    /// <returns>
    /// If 0 is returned, write is successful, and sink is ready to accept more data <br/>
    /// If -1 is returned, write is not successful, and no further data will be accepted
    /// (usually encoding or protocol error) <br/>
    /// If a positive number is returned, write is partially successful,
    /// and WaitAsync should be called and awaited due to flow control reasons
    /// </returns>
    int TryWrite(Span<T> data);

    /// <summary>
    /// Wait until more data can be supplied to the processing stage or pipeline has failed.
    /// Usually called at the start of a steaming transfer or after a StreamSinkResult.Block returned from TryWrite.
    /// Caller should supply the amount of data pending to be written via sizeHint. Implementation should always
    /// consider the sizeHint and unblock only if sizeHint can be written through or when enough of capacity has been
    /// freed up to allow an efficient enough write to happen. <br/>
    /// No exceptions other than fatal ones and OutOfMemory is allowed to exit this call.
    /// </summary>
    /// <param name="sizeHint"> See summary </param>
    /// <returns> Async task handle </returns>
    ValueTask WaitAsync(int sizeHint);

    /// <summary>
    /// Indicate that no further data will be available for now.
    /// Remaining data should be processed and sent downstream if necessary
    /// </summary>
    /// <returns> Async task handle </returns>
    ValueTask FlushAsync();

    /// <summary>
    /// Indicate that no further data will be available.
    /// Additional data to explain the shutdown could be passed with the reason parameter. <br/>
    /// This function should be used for cleanup only and should never raise exceptions.
    /// </summary>
    /// <param name="reason"> An optional reason to explain the completion (i.e. Exception) </param>
    void Complete(object? reason = null);
}

public class PolymorphicStreamSinkWrapper<T, TO>(TO next) : IStreamSink<T>
    where TO : struct, IStreamSink<T>
    where T : unmanaged
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TryWrite(Span<T> data) => next.TryWrite(data);
    
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask WaitAsync(int sizeHint) => next.WaitAsync(sizeHint);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask FlushAsync() => next.FlushAsync();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Complete(object? reason = null) => next.Complete(reason);
}

public readonly struct PolymorphicStreamConnector<T>(IStreamSink<T> next) : IStreamSink<T> where T : unmanaged
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TryWrite(Span<T> data) => next.TryWrite(data);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask WaitAsync(int sizeHint) => next.WaitAsync(sizeHint);
    
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask FlushAsync() => next.FlushAsync();
    
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Complete(object? reason = null) => next.Complete(reason);
}
