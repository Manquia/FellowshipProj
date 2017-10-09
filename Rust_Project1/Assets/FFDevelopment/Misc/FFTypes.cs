


public enum Trinary
{
    True,
    False,
    Null,
}

public class Annal<Type>
{
    public Type[] data;
    public int currentIndex;

    Annal(int annalSize)
    {
        data = new Type[annalSize];
        currentIndex = 0;
    }
    
    public static implicit operator Type(Annal<Type> d)
    {
        return d.data[d.currentIndex];
    }
    public void Push(Type annal)
    {
        currentIndex = ++currentIndex % data.Length;
        data[currentIndex] = annal;
    }
}
