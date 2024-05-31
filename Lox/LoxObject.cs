using Lox.Lang;

namespace Lox;

public class LoxObject
{
    protected bool Equals(LoxObject other)
    {
        return Nullable.Equals(this.number, other.number) 
               && this.boolean == other.boolean 
               && this.str == other.str 
               && this.list.Equals(other.list) 
               && Equals(this.instance, other.instance) 
               && Equals(this.callable, other.callable) 
               && Equals(this.@class, other.@class) 
               && LoxType == other.LoxType;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((LoxObject)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.number, this.boolean, this.str, this.list, this.instance, this.callable, this.@class, (int)LoxType);
    }

    public static readonly LoxObject Nil = new();
    public static readonly LoxObject True = new(true);
    public static readonly LoxObject False = new(false);

    private double? number = null;
    private bool? boolean = null;
    private string? str = null;
    private LoxList list = null;
    private LoxInstance? instance;
    private LoxNativeInstance? nativeInstance;
    private ILoxCallable? callable;
    private LoxClass? @class;

    private LoxObject()
    {
        this.LoxType = LoxType.Nil;
    }

    public LoxObject(LoxNativeInstance nativeInstance)
    {
        this.nativeInstance = nativeInstance;
        this.LoxType = LoxType.NativeInstance;
    }

    public LoxObject(LoxClass @class)
    {
        this.@class = @class;
        this.LoxType = LoxType.Class;
    }

    public LoxObject(ILoxCallable callable)
    {
        this.callable = callable;
        this.LoxType = LoxType.Callable;
    }

    public LoxObject(LoxInstance instance)
    {
        this.instance = instance;
        this.LoxType = LoxType.Instance;
    }

    public LoxObject(bool boolean)
    {
        this.boolean = boolean;
        this.LoxType = LoxType.Bool;
    }

    public LoxObject(string str)
    {
        this.str = str;
        this.LoxType = LoxType.Str;
    }

    public LoxObject(double num)
    {
        this.number = num;
        this.LoxType = LoxType.Num;
    }

    public LoxObject(List<LoxObject> list)
    {
        this.LoxType = LoxType.List;
        this.list = new LoxList(list);
    }

    public LoxObject(LoxList list)
    {
        this.LoxType = LoxType.List;
        this.list = list;
    }

    public LoxObject(Token token)
    {
        switch (token.Type)
        {
            case TokenType.NUMBER:
                this.LoxType = LoxType.Num;
                this.number = double.Parse(token.Lexeme);
                break;
            case TokenType.STRING:
                this.LoxType = LoxType.Str;
                this.str = token.Lexeme;
                break;
            case TokenType.TRUE:
                this.LoxType = LoxType.Bool;
                this.boolean = true;
                break;
            case TokenType.FALSE:
                this.LoxType = LoxType.Bool;
                this.boolean = false;
                break;
            case TokenType.NIL:
                this.LoxType = LoxType.Nil;
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
    public bool IsCallable => LoxType == LoxType.Callable;
    public bool IsInstance => LoxType == LoxType.Instance;
    public bool IsNativeInstance => LoxType == LoxType.NativeInstance;

    public double? AsNumber() => this.number;

    public double GetNumber()
    {
        if (IsNumber)
            return this.number.GetValueOrDefault();

        throw new RuntimeError("LoxObject is not a number.");
    }

    public string? AsString() => this.str;

    public string GetString()
    {
        if (IsStr)
            return this.str!;

        throw new RuntimeError("LoxObject is not a string.");
    }

    public bool? AsBool() => this.boolean;

    public bool GetBool()
    {
        if (IsBool)
            return this.boolean!.Value;

        throw new RuntimeError("LoxObject is not a boolean.");
    }

    public ILoxCallable? AsCallable() => this.callable;

    public ILoxCallable GetCallable()
    {
        if (IsCallable)
            return this.callable!;

        throw new RuntimeError("LoxObject is not a callable.");
    }

    public LoxInstance? AsLoxInstance() => this.instance;

    public LoxInstance GetLoxInstance()
    {
        if (IsInstance)
            return this.instance!;

        throw new RuntimeError("LoxObject is not a lox instance.");
    }

    public LoxClass? AsClass() => this.@class;

    public LoxClass GetClass()
    {
        if (IsClass)
            return this.@class!;

        throw new RuntimeError("LoxObject is not a lox class.");
    }

    public LoxList AsList() => this.list;

    public LoxList GetList()
    {
        if (IsList)
            return this.list!;

        throw new RuntimeError("LoxObject is not a list.");
    }

    public LoxNativeInstance? AsNativeInstance()
    {
        return this.nativeInstance!; 
    }

    public LoxNativeInstance GetNativeInstance()
    {
        if (IsNativeInstance)
            return this.nativeInstance!;

        throw new RuntimeError("LoxObject is not a native instance.");
    }

    public bool IsTruthy()
    {
        if (IsBool) return this.boolean!.Value;
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
            LoxType.Num => this.number!.ToString(),
            LoxType.Bool => this.boolean!.ToString(),
            LoxType.Callable => this.callable!.ToString(),
            LoxType.Class => $"<class {this.@class!.Name}>",
            LoxType.Instance => $"<instance {this.instance!.ToString()}>",
            LoxType.List => $"<list {this.list!.ToString()}>",
            LoxType.Nil => "nil",
            LoxType.Str => this.str!,
            _ => "Unknown type"
        };

        return result ?? string.Empty;
    }
}