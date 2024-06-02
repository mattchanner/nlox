using System.Text;
using Lox.Parser;
using Microsoft.CSharp.RuntimeBinder;

namespace Lox.Runtime;

public class LoxList
{
    public LoxList(List<LoxObject?> items)
    {
        Items = new List<LoxObject>(items);
    }

    public LoxList(LoxList first, LoxList second)
    {
        Items = new List<LoxObject>();
        Items.AddRange(first.Items);
        Items.AddRange(second.Items);
    }

    public LoxList(LoxList first, params LoxObject[] rest)
    {
        Items = new List<LoxObject>();
        Items.AddRange(first.Items);
        Items.AddRange(rest);
    }

    public LoxObject Count()
    {
        return new LoxObject(Items.Count);
    }

    public LoxObject At(int index)
    {
        return Items[index];
    }

    public LoxObject Remove(int index)
    {
        LoxObject value = Items[index];
        Items.RemoveAt(index);
        return value;
    }

    public void Add(LoxObject item)
    {
        Items.Add(item);
    }

    protected bool Equals(LoxList other)
    {
        return Items.Equals(other.Items);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LoxList)obj);
    }

    public override int GetHashCode()
    {
        return Items.GetHashCode();
    }

    public List<LoxObject> Items { get; init; }

    public static bool operator ==(LoxList left, LoxList right)
    {
        if (left.Items.Count != right.Items.Count) return false;
        return left.Items.SequenceEqual(right.Items);
    }

    public static bool operator !=(LoxList left, LoxList right)
    {
        return !left.Items.SequenceEqual(right.Items);
    }

    public override string ToString()
    {
        StringBuilder b = new();
        b.Append('[');
        b.Append(string.Join(",", Items));
        b.Append(']');

        return b.ToString();
    }

    public LoxObject? Get(Token name)
    {
        return name.Lexeme switch
        {
            "at" => new LoxObject(new AtWrapper(this)),
            "count" => new LoxObject(new CountWrapper(this)),
            "remove" => new LoxObject(new RemoveWrapper(this)),
            "add" => new LoxObject(new AddWrapper(this)),
            _ => throw new RuntimeError($"List does not contain a method named '{name.Lexeme}'")
        };
    }
}

class AtWrapper(LoxList list) : ILoxCallable
{
    public int Arity => 1;
    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        int index = (int)arguments[0]!.GetNumber();
        return list.At(index);
    }
}

class RemoveWrapper(LoxList list) : ILoxCallable
{
    public int Arity => 1;
    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        int index = (int)arguments[0]!.GetNumber();
        return list.Remove(index);
    }
}

class CountWrapper(LoxList list) : ILoxCallable
{
    public int Arity => 0;
    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        return new LoxObject(list.Items.Count);
    }
}

class AddWrapper(LoxList list) : ILoxCallable
{
    public int Arity => 1;
    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        list.Add(arguments[0]!);
        return LoxObject.Nil;
    }
}
