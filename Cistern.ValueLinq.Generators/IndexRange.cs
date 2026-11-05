namespace System;

// netstandard2.0 has neither type; the compiler binds list patterns to these by name and shape
internal readonly struct Index : IEquatable<Index>
{
    // a from-end index is stored as its bitwise complement, so the sign carries the direction
    private readonly int _value;

    public Index(int value, bool fromEnd = false)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "An index must be non-negative.");

        _value =
            fromEnd
                ? ~value
                : value;
    }

    private Index(int value) => _value = value;

    public static Index Start => new(0);
    public static Index End => new(~0);

    public static Index FromStart(int value) => new(value, fromEnd: false);
    public static Index FromEnd(int value) => new(value, fromEnd: true);

    public int Value
    {
        get
        {
            int value =
                _value < 0
                    ? ~_value
                    : _value;
            return value;
        }
    }

    public bool IsFromEnd => _value < 0;

    public int GetOffset(int length)
    {
        int offset =
            IsFromEnd
                ? _value + length + 1
                : _value;
        return offset;
    }

    public bool Equals(Index other) => _value == other._value;

    public override bool Equals(object? other) => other is Index index && _value == index._value;

    public override int GetHashCode() => _value;

    public override string ToString()
    {
        var text =
            IsFromEnd
                ? $"^{Value}"
                : $"{Value}";
        return text;
    }

    public static implicit operator Index(int value) => FromStart(value);
}

internal readonly struct Range : IEquatable<Range>
{
    public Index Start { get; }
    public Index End { get; }

    public Range(Index start, Index end)
    {
        Start = start;
        End = end;
    }

    public static Range StartAt(Index start) => new(start, Index.End);
    public static Range EndAt(Index end) => new(Index.Start, end);
    public static Range All => new(Index.Start, Index.End);

    public (int Offset, int Length) GetOffsetAndLength(int length)
    {
        int start = Start.GetOffset(length);
        int end = End.GetOffset(length);
        if ((uint)end > (uint)length
            || (uint)start > (uint)end)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "The range lies outside the length.");
        }

        return (start, end - start);
    }

    public bool Equals(Range other) => Start.Equals(other.Start) && End.Equals(other.End);

    public override bool Equals(object? other) => other is Range range && Equals(range);

    public override int GetHashCode() => (Start.GetHashCode() * 31) + End.GetHashCode();

    public override string ToString() => $"{Start}..{End}";
}
