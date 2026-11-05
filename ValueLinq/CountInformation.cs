namespace Cistern.ValueLinq;

using System;

public struct CountInformation
{
    enum Flags : byte
    {
        HasMaximumLength                = 0b_0000001,
        ActualLengthIsMaximumLength     = 0b_0000010,
        LengthIsImmutable               = 0b_0000100,
        IsStale                         = 0b_0001000,
        PotentialSideEffects            = 0b_0010000,
        CountingDepth                   = 0b_0100000,
        O1Reversal                      = 0b_1000000,
    }

    public CountInformation(long? size, bool isImmutable)
    {
        _flags = Flags.CountingDepth;

        _maybeMaximumLength = size.GetValueOrDefault();
        if (size.HasValue)
            _flags |= Flags.HasMaximumLength | Flags.ActualLengthIsMaximumLength;

        if (isImmutable)
            _flags |= Flags.LengthIsImmutable;

        _depth = 0;
    }

    private CountInformation(long maybeMaximumLength, Flags flags, byte depth)
    {
        _maybeMaximumLength = maybeMaximumLength;
        _flags = flags;
        _depth = depth;
    }

    internal static CountInformation Concat(in CountInformation start, in CountInformation finish)
    {
        var both = start._flags & finish._flags;
        var flags = both & (Flags.HasMaximumLength | Flags.ActualLengthIsMaximumLength | Flags.LengthIsImmutable | Flags.CountingDepth);
        if ((both & Flags.LengthIsImmutable) == 0)
            flags |= Flags.IsStale;
#if PROPERLY_RIPPLE_POTENTIAL_SIDE_EFFECTS
        flags |= (start._flags | finish._flags) & Flags.PotentialSideEffects;
#endif

        var maybeMaximumLength = (flags & Flags.HasMaximumLength) != 0 ? start._maybeMaximumLength + finish._maybeMaximumLength : 0;
        var depth = (flags & Flags.CountingDepth) != 0 ? (byte)(Math.Max(start._depth, finish._depth) + 1) : byte.MaxValue;
        return new CountInformation(maybeMaximumLength, flags, depth);
    }

    long _maybeMaximumLength;
    // tested with & rather than HasFlag, whose IL puts a getter over the size the JIT always inlines
    Flags _flags;
    byte _depth;

    // a length cached from a collection that can change (IsStale) may no longer be the sequence's
    public readonly int? ActualSize =>
        ActualLengthIsMaximumLength && !IsStale
            ? (int?)MaximumLength
            : null;

    /// <summary>
    /// The maximum length of the sequence. It is null if the maximum length is unknown.
    /// </summary>
    public long? MaximumLength
    {
        readonly get => (_flags & Flags.HasMaximumLength) != 0 ? (long?)_maybeMaximumLength : null;
        set {
            _maybeMaximumLength = value.GetValueOrDefault();
            if (value.HasValue) _flags |= Flags.HasMaximumLength; else _flags &= ~Flags.HasMaximumLength;
        }
    }

    public int? Depth
    {
        readonly get => (_flags & Flags.CountingDepth) != 0 ? (int?)_depth : null;
        set
        {
            if (value >= 0 || value <= byte.MaxValue)
            {
                _flags |= Flags.CountingDepth;
                _depth = (byte)value;
            }
            else
            {
                _flags &= ~Flags.CountingDepth;
                _depth = byte.MaxValue;
            }
        }
    }

    /// <summary>
    /// MaximumLength contains the true length of the sequence when this is true. It is false if the
    /// stream passed through a filtering function such as Where.
    /// </summary>
    public bool ActualLengthIsMaximumLength
    {
        readonly get => (_flags & Flags.ActualLengthIsMaximumLength) != 0;
        set { if (value) _flags |= Flags.ActualLengthIsMaximumLength; else _flags &= ~Flags.ActualLengthIsMaximumLength; }
    }

    /// <summary>
    /// If the length of the underlying containers length is immutable, such as an array, or rather than an
    /// enumerable that could change size such as a List
    /// </summary>
    public bool LengthIsImmutable
    {
        readonly get => (_flags & Flags.LengthIsImmutable) != 0;
        set { if (value) _flags |= Flags.LengthIsImmutable; else _flags &= ~Flags.LengthIsImmutable; }
    }

    /// <summary>
    /// When the data has been cached on a node, such as Concat. Because potentially the underlying structure
    /// might have changed, such as with a List
    /// </summary>
    public bool IsStale
    {
        readonly get => (_flags & Flags.IsStale) != 0;
        set { if (value) _flags |= Flags.IsStale; else _flags &= ~Flags.IsStale; }
    }

    /// <summary>
    /// Lamdbda's passed to Select are considered as having side effects so in a usual Count() instruction they
    /// are enumerated, athough System.Linq's implemetation doesn't handle this consistently (i.e. if a Select
    /// is then contained within a Concat as an example)
    /// </summary>
    public bool PotentialSideEffects
    {
        readonly get => (_flags & Flags.PotentialSideEffects) != 0;
        set { if (value) _flags |= Flags.PotentialSideEffects; else _flags &= ~Flags.PotentialSideEffects; }
    }
}
