using System.Text;

using Lox.Lang;
using Microsoft.CSharp.RuntimeBinder;

namespace Lox;

public class LoxList
{
    public LoxList(List<LoxObject?> items)
    {
        this.Items = items;
    }

    public LoxList(LoxList first, LoxList second)
    {
        this.Items = new List<LoxObject>();
        this.Items.AddRange(first.Items);
        this.Items.AddRange(second.Items);
    }

    public LoxList(LoxList first, params LoxObject[] rest)
    {
        this.Items = new List<LoxObject>();
        this.Items.AddRange(first.Items);
        this.Items.AddRange(rest);
    }

    public LoxObject Count()
    {
        return new LoxObject(Items.Count);
    }

    public LoxObject At(int index)
    {
        return this.Items[index];
    }

    public LoxObject Remove(int index)
    {
        LoxObject value = this.Items[index];
        this.Items.RemoveAt(index);
        return value;
    }

    public void Add(LoxObject item)
    {
        this.Items.Add(item);
    }

    protected bool Equals(LoxList other)
    {
        return Items.Equals(other.Items);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
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
