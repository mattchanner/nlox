using Lox.Parser;

namespace Lox.Runtime;

public class LoxObject
{
    protected bool Equals(LoxObject other)
    {
        bool ListEquals()
        {
            if (ReferenceEquals(null, list)) return ReferenceEquals(null, other.list);
            if (ReferenceEquals(null, other.list)) return false;
            return this.list.Equals(other.list);
        }

        return Nullable.Equals(number, other.number)
               && boolean == other.boolean
               && str == other.str
               && ListEquals()
               && Equals(instance, other.instance)
               && Equals(callable, other.callable)
               && Equals(@class, other.@class)
               && LoxType == other.LoxType;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LoxObject)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            number, 
            boolean, 
            str, 
            list, 
            instance, 
            callable, 
            @class, 
            (int)LoxType);
    }

    public static readonly LoxObject Nil = new();
    public static readonly LoxObject True = new(true);
    public static readonly LoxObject False = new(false);

    private double? number = null;
    private bool? boolean = null;
    private readonly string? str = null;
    private readonly LoxList? list = null;
    private readonly LoxInstance? instance;
    private readonly LoxNativeInstance? nativeInstance;
    private readonly ILoxCallable? callable;
    private readonly LoxClass? @class;

    private LoxObject()
    {
        LoxType = LoxType.Nil;
    }

    public LoxObject(LoxNativeInstance nativeInstance)
    {
        this.nativeInstance = nativeInstance;
        LoxType = LoxType.NativeInstance;
    }

    public LoxObject(LoxClass @class)
    {
        this.@class = @class;
        LoxType = LoxType.Class;
    }

    public LoxObject(ILoxCallable callable)
    {
        this.callable = callable;
        LoxType = LoxType.Callable;
    }

    public LoxObject(LoxInstance instance)
    {
        this.instance = instance;
        LoxType = LoxType.Instance;
    }

    public LoxObject(bool boolean)
    {
        this.boolean = boolean;
        LoxType = LoxType.Bool;
    }

    public LoxObject(string str)
    {
        this.str = str;
        LoxType = LoxType.Str;
    }

    public LoxObject(double num)
    {
        number = num;
        LoxType = LoxType.Num;
    }

    public LoxObject(List<LoxObject> list)
    {
        LoxType = LoxType.List;
        this.list = new LoxList(list);
    }

    public LoxObject(LoxList list)
    {
        LoxType = LoxType.List;
        this.list = list;
    }

    public LoxObject(Token token)
    {
        switch (token.Type)
        {
            case TokenType.NUMBER:
                LoxType = LoxType.Num;
                number = double.Parse(token.Lexeme);
                break;
            case TokenType.STRING:
                LoxType = LoxType.Str;
                str = token.Lexeme;
                break;
            case TokenType.TRUE:
                LoxType = LoxType.Bool;
                boolean = true;
                break;
            case TokenType.FALSE:
                LoxType = LoxType.Bool;
                boolean = false;
                break;
            case TokenType.NIL:
                LoxType = LoxType.Nil;
                break;
            default:
                throw new RuntimeError(token, "Invalid token provided when constructing an object.");
        }
    }

    public LoxType LoxType { get; }

    public bool IsClass => LoxType == LoxType.Class;
    public bool IsNumber => LoxType == LoxType.Num;
    public bool IsStr => LoxType == LoxType.Str;
    public bool IsNil => LoxType == LoxType.Nil;
    public bool IsBool => LoxType == LoxType.Bool;
    public bool IsList => LoxType == LoxType.List;
    public bool IsCallable => LoxType is LoxType.Callable or LoxType.Class;
    public bool IsInstance => LoxType == LoxType.Instance;
    public bool IsNativeInstance => LoxType == LoxType.NativeInstance;

    public double? AsNumber() => number;

    public double GetNumber()
    {
        if (IsNumber)
            return number.GetValueOrDefault();

        throw new RuntimeError("LoxObject is not a number.");
    }

    public string? AsString() => str;

    public string GetString()
    {
        if (IsStr)
            return str!;

        throw new RuntimeError("LoxObject is not a string.");
    }

    public bool? AsBool() => boolean;

    public bool GetBool()
    {
        if (IsBool)
            return boolean!.Value;

        throw new RuntimeError("LoxObject is not a boolean.");
    }

    public ILoxCallable? AsCallable() => callable ?? @class;

    public ILoxCallable GetCallable()
    {
        if (IsCallable)
            return callable ?? @class;

        throw new RuntimeError("LoxObject is not a callable.");
    }

    public LoxInstance? AsLoxInstance() => instance;

    public LoxInstance GetLoxInstance()
    {
        if (IsInstance)
            return instance!;

        throw new RuntimeError("LoxObject is not a lox instance.");
    }

    public LoxClass? AsClass() => @class;

    public LoxClass GetClass()
    {
        if (IsClass)
            return @class!;

        throw new RuntimeError("LoxObject is not a lox class.");
    }

    public LoxList AsList() => list;

    public LoxList GetList()
    {
        if (IsList)
            return list!;

        throw new RuntimeError("LoxObject is not a list.");
    }

    public LoxNativeInstance? AsNativeInstance()
    {
        return nativeInstance!;
    }

    public LoxNativeInstance GetNativeInstance()
    {
        if (IsNativeInstance)
            return nativeInstance!;

        throw new RuntimeError("LoxObject is not a native instance.");
    }

    public bool IsTruthy()
    {
        if (IsBool) return boolean!.Value;
        return !IsNil;
    }

    public LoxObject Power(LoxObject other)
    {
        if (IsNumber && other.IsNumber)
            return new LoxObject(Math.Pow(GetNumber(), other.GetNumber()));

        throw new RuntimeError($"Cannot apply operator '^' to operands of type {LoxType} and {other.LoxType}");
    }

    public LoxObject Modulus(LoxObject other)
    {
        if (IsNumber && other.IsNumber)
            return new LoxObject(GetNumber() % other.GetNumber());

        throw new RuntimeError($"Cannot apply operator '%' to operands of type {LoxType} and {other.LoxType}");
    }

    public static LoxObject operator +(LoxObject left, LoxObject right)
    {
        if (left.IsNumber && right.IsNumber)
            return new LoxObject(left.GetNumber() + right.GetNumber());

        if (left.IsStr && right.IsStr)
            return new LoxObject(left.GetString() + right.GetString());

        if (left.IsList && right.IsList)
            return new LoxObject(new LoxList(left.GetList(), right.GetList()));

        if (left.IsList)
            return new LoxObject(new LoxList(left.GetList(), right));

        throw new RuntimeError($"Cannot apply operator '+' to operands of type {left.LoxType} and {right.LoxType}");
    }

    public static LoxObject operator -(LoxObject left, LoxObject right)
    {
        if (left.IsNumber && right.IsNumber)
            return new LoxObject(left.GetNumber() - right.GetNumber());

        throw new RuntimeError($"Cannot apply operator '-' to operands of type {left.LoxType} and {right.LoxType}");
    }

    public static LoxObject operator /(LoxObject left, LoxObject right)
    {
        if (left.IsNumber && right.IsNumber)
            return new LoxObject(left.GetNumber() / right.GetNumber());

        throw new RuntimeError($"Cannot apply operator '/' to operands of type {left.LoxType} and {right.LoxType}");
    }

    public static LoxObject operator *(LoxObject left, LoxObject right)
    {
        if (left.IsNumber && right.IsNumber)
            return new LoxObject(left.GetNumber() * right.GetNumber());

        throw new RuntimeError($"Cannot apply operator '*' to operands of type {left.LoxType} and {right.LoxType}");
    }

    public static bool operator ==(LoxObject left, LoxObject right)
    {
        if (ReferenceEquals(null, left))
            return ReferenceEquals(null, right);

        if (ReferenceEquals(null, right))
            return false;

        if (left.LoxType != right.LoxType) return false;
        return left.LoxType switch
        {
            LoxType.Num => Math.Abs(left.GetNumber() - right.GetNumber()) < 0.00001,
            LoxType.Bool => left.GetBool() == right.GetBool(),
            LoxType.Callable => left.GetCallable() == right.GetCallable(),
            LoxType.Instance => left.GetLoxInstance() == right.GetLoxInstance(),
            LoxType.Str => left.GetString() == right.GetString(),
            LoxType.Nil => true,
            LoxType.List => left.GetList() == right.GetList(),
            LoxType.Class => left.GetClass() == right.GetClass(),
            _ => false
        };
    }

    public static bool operator !=(LoxObject left, LoxObject right)
    {
        return !(left == right);
    }

    public static bool operator >(LoxObject left, LoxObject right)
    {
        if (left.IsNumber && right.IsNumber)
            return left.GetNumber() > right.GetNumber();

        if (left.IsStr && right.IsStr)
            return string.CompareOrdinal(left.GetString(), right.GetString()) > 0;

        throw new RuntimeError($"Cannot apply operator '>' to operands of type {left.LoxType} and {right.LoxType}");
    }

    public static bool operator <(LoxObject left, LoxObject right)
    {
        if (left.IsNumber && right.IsNumber)
            return left.GetNumber() < right.GetNumber();

        if (left.IsStr && right.IsStr)
            return string.CompareOrdinal(left.GetString(), right.GetString()) < 0;

        throw new RuntimeError($"Cannot apply operator '<' to operands of type {left.LoxType} and {right.LoxType}");
    }

    public static bool operator >=(LoxObject left, LoxObject right)
    {
        if (left.IsNumber && right.IsNumber)
            return left.GetNumber() >= right.GetNumber();

        if (left.IsStr && right.IsStr)
            return string.CompareOrdinal(left.GetString(), right.GetString()) >= 0;

        throw new RuntimeError($"Cannot apply operator '>=' to operands of type {left.LoxType} and {right.LoxType}");
    }

    public static bool operator <=(LoxObject left, LoxObject right)
    {
        if (left.IsNumber && right.IsNumber)
            return left.GetNumber() < right.GetNumber();

        if (left.IsStr && right.IsStr)
            return string.CompareOrdinal(left.GetString(), right.GetString()) <= 0;

        throw new RuntimeError($"Cannot apply operator '<=' to operands of type {left.LoxType} and {right.LoxType}");
    }

    public static LoxObject operator -(LoxObject right)
    {
        if (right.IsNumber)
            return new LoxObject(-right.GetNumber());

        throw new RuntimeError($"Cannot apply operator '-' to operands of type {right.LoxType}");
    }

    public static LoxObject operator !(LoxObject right)
    {
        return new LoxObject(!right.IsTruthy());
    }

    public static LoxObject operator ++(LoxObject right)
    {
        if (right.IsNumber)
        {
            right.number += 1;
            return right;
        }

        throw new RuntimeError($"Cannot apply operator '++' to operands of type {right.LoxType}");
    }

    public static LoxObject operator --(LoxObject right)
    {
        if (right.IsNumber)
        {
            right.number -= 1;
            return right;
        }

        throw new RuntimeError($"Cannot apply operator '--' to operands of type {right.LoxType}");
    }

    public override string ToString()
    {
        string? result = LoxType switch
        {
            LoxType.Num => number!.ToString(),
            LoxType.Bool => boolean!.ToString(),
            LoxType.Callable => callable!.ToString(),
            LoxType.Class => $"<class {@class!.Name}>",
            LoxType.Instance => $"<instance {instance!.ToString()}>",
            LoxType.List => $"<list {list!.ToString()}>",
            LoxType.Nil => "nil",
            LoxType.Str => str!,
            _ => "Unknown type"
        };

        return result ?? string.Empty;
    }
}