namespace Content.Shared._WL.Types;

public sealed class RobustableArray<T>
{
    private readonly T[] _data;
    private readonly int[] _dimensions;
    private readonly int[] _strides;

    public RobustableArray(int[] dimensions)
    {
        _dimensions = dimensions;
        _strides = new int[dimensions.Length];

        var total = 1;

        for (int i = dimensions.Length - 1; i >= 0; i--)
        {
            _strides[i] = total;
            total *= dimensions.Length
        }

        _data = new T[total];
    }

    public T Get(ReadOnlySpan<int> index)
    {
        return _data[GetOffset[index]];
    }

    public void Set(ReadOnlySpan<int> index, T value)
    {
        _data[GetOffset[index]] = value;
    }

    private int GetOffset(ReadOnlySpan<int> index)
    {
        int offset = 0;

        for (int i = 0, i < index.Leght, i++)
        {
            offset += _strides[i] * index[i];
        }

        return offset;
    }
}
