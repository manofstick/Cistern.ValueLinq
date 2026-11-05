using Cistern.ValueLinq.Chain;
using System;

namespace Cistern.ValueLinq.ValueEnumerable
{
    // what foreach holds: the chain by value; Current is read from it once per element and is default after the end,
    // the chain is not pulled again after its end, and it is disposed by Dispose alone, not on reaching its end (see
    // Docs/PullProtocol.md). A chain whose rebuild merges a Skip or Take into a buffering node's window is held as that
    // rebuilt chain's heap enumerator instead, and _enumerator is left default
    public struct ValueEnumerator<T, TEnumerator>
        : IDisposable
        where TEnumerator : struct, IPullEnumerator<T>
    {
        private TEnumerator _enumerator;
        private readonly PullEnumerator<T>? _pull;
        [AllowNull] private T _current;
        private bool _finished;
        private bool _disposed;

        internal ValueEnumerator(in TEnumerator enumerator)
        {
            _enumerator = enumerator;
            _pull = null;
            _current = default;
            _finished = false;
            _disposed = false;
        }

        internal ValueEnumerator(PullEnumerator<T> pull)
        {
            _enumerator = default;
            _pull = pull;
            _current = default;
            _finished = false;
            _disposed = false;
        }

        public readonly T Current => _current;

        public void Dispose()
        {
            if (TEnumerator.PreferRebuilt)
            {
                _pull!.Dispose();
                return;
            }

            if (_disposed)
                return;
            _disposed = true;
            _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            if (TEnumerator.PreferRebuilt)
                return _pull!.TryGetNext(out _current);

            if (_finished)
                return false;
            if (_enumerator.MoveNext())
            {
                _current = _enumerator.Current;
                return true;
            }
            _current = default;
            _finished = true;
            return false;
        }
    }
}
