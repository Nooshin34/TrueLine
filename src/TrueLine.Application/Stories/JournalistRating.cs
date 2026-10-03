namespace TrueLine.Application.Stories;

public static class JournalistRating
{
    public static int Stars(int totalReads) => totalReads switch
    {
        <= 0 => 0,
        <= 4 => 1,
        <= 14 => 2,
        <= 39 => 3,
        <= 99 => 4,
        _ => 5,
    };
}
