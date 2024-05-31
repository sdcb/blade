namespace SpinBladeArena.Primitives;

public class RandomNumbersGenerator
{
    private static readonly Random random = new();

    public static int[] GetRandomNumbers(int from, int to, int count)
    {
        if (count > to - from)
        {
            throw new ArgumentException("count cannot be greater than the range [from, to).");
        }

        // Generate all numbers in the range [from, to)
        List<int> numbers = Enumerable.Range(from, to - from).ToList();

        // Use Fisher-Yates shuffle algorithm to get the first 'count' elements
        for (int i = 0; i < count; i++)
        {
            int j = random.Next(i, numbers.Count);
            int temp = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = temp;
        }

        return numbers.Take(count).ToArray();
    }

}
