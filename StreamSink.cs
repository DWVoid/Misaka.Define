namespace Misaka.Define;

public enum StreamSinkResult
{
    /// <summary>
    /// Write is successful, and sink is ready to accept more data
    /// </summary>
    Open,
    /// <summary>
    /// Write is not successful, and no further data will be accepted (usually encoding or protocol error)
    /// </summary>
    Close,
    /// <summary>
    /// Write is successful, WaitAsync should be called and awaited due to flow control reasons
    /// </summary>
    Block
}

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
    /// Implementers are expected to process data during this call.
    /// No exceptions other than fatal ones and OutOfMemory is allowed to exit this call.
    /// </summary>
    /// <param name="data"> The supplied data </param>
    /// <returns> Current state of the sink </returns>
    StreamSinkResult TryWrite(Span<T> data);

    /// <summary>
    /// Wait until more data can be supplied to the processing stage or pipeline has failed.
    /// Usually called at the start of a steaming transfer or after a StreamSinkResult.Block returned from TryWrite.
    /// No exceptions other than fatal ones and OutOfMemory is allowed to exit this call.
    /// </summary>
    /// <returns> Async task handle </returns>
    ValueTask WaitAsync();

    /// <summary>
    /// Indicate that no further data will be available for now.
    /// Remaining data should be processed and sent downstream if necessary
    /// </summary>
    /// <returns> Async task handle </returns>
    ValueTask FlushAsync();
}