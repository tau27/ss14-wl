using System.Text;

namespace Content.Shared._WL.Languages;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ObfuscationMethod
{
    public static readonly ObfuscationMethod Default = new ReplacementObfuscation
    {
        Replacement = new List<string> { "<?>" }
    };

    internal int PseudoRandom(int seed, int global_seed, int start, int end)
    {
        int result = 0;
        int gap = end - start + 1;
        result = seed ^ (global_seed * 255);
        result = System.Math.Abs((result + 619251) * 27644437);
        result %= gap;
        result += start;
        return result;
    }

    internal abstract void Obfuscate(StringBuilder builder, string message, int global_seed);

    public string Obfuscate(string message, int global_seed)
    {
        var builder = new StringBuilder();
        Obfuscate(builder, message, global_seed);
        return builder.ToString();
    }

    public abstract bool IsEmoting();
}

public partial class ReplacementObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public List<string> Replacement = [];

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        var index = PseudoRandom(message.GetHashCode(), global_seed, 0, Replacement.Count - 1);
        builder.Append(Replacement[index]);
    }

    public override bool IsEmoting()
    {
        return false;
    }
}

public partial class WordsReplacementObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public List<string> Replacement = [];

    internal bool IsPunct(char ch)
    {
        List<char> punctuation = new List<char>() {'.', ',', ';', ':', '!', '?'};
        return punctuation.Contains(ch);
    }

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        int buffer = 0;
        const char eof = (char) 0;
        for (int i = 0; i <= message.Length; i++)
        {
            var ch = i < message.Length ? message[i] : eof;
            if ((IsPunct(ch) || ch == ' ' || ch == eof))
            {
                if (buffer > 0)
                {
                    var index = PseudoRandom(buffer, global_seed, 0, Replacement.Count - 1);
                    builder.Append(Replacement[index]);
                    buffer = 0;
                }

                if (ch != eof)
                {
                    builder.Append(ch);
                }
            }
            else
            {
                buffer += buffer * 41 + ch;
            }
        }
    }

    public override bool IsEmoting()
    {
        return false;
    }
}

public partial class ByCharReplacementObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public List<string> Replacement = [];

    [DataField]
    public bool randlength = true;

    [DataField]
    public int maxlength = 10;

    internal bool IsPunct(char ch)
    {
        List<char> punctuation = new List<char>() {'.', ',', ';', ':', '!', '?'};
        return punctuation.Contains(ch);
    }

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        int buffer = 0;
        int counter = 0;
        const char eof = (char) 0;
        for (int i = 0; i <= message.Length; i++)
        {
            var ch = i < message.Length ? message[i] : eof;
            if ((IsPunct(ch) || ch == ' ' || ch == eof))
            {
                if (buffer > 0)
                {
                    var length = randlength ? PseudoRandom(buffer, global_seed, 1, maxlength) : counter;
                    for (int j = 0; j < length; j++)
                    {
                        var index = PseudoRandom(buffer*(j+3)+j, global_seed, 0, Replacement.Count - 1);
                        builder.Append(Replacement[index]);
                    }

                    buffer = 0;
                    counter = 0;
                }

                if (ch != eof)
                {
                    builder.Append(ch);
                }
            }
            else
            {
                buffer += buffer * 41 + ch;
                counter++;
            }
        }
    }

    public override bool IsEmoting()
    {
        return false;
    }
}
public partial class EmoteObfuscation : ObfuscationMethod
{
    [DataField(required: true)]
    public List<string> Replacement = [];

    [DataField]
    public int min = 1;

    [DataField]
    public int max = 30;

    internal override void Obfuscate(StringBuilder builder, string message, int global_seed)
    {
        var gap = max - min;
        var index = (int)((message.Length - min) / gap * Replacement.Count);
        index = System.Math.Min(0, System.Math.Max(index, Replacement.Count - 1));
        builder.Append(Replacement[index]);
    }

    public override bool IsEmoting()
    {
        return true;
    }
}
