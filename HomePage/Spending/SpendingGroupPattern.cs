namespace HomePage.Spending
{
    public class SpendingGroupPattern(string pattern)
    {
        private readonly string pattern = pattern;

        public static SpendingGroupPattern FromString(string pattern) => new(pattern);

        public bool Matches(string s)
        {
            var sourceString = pattern.ToLower();
            var targetString = s.ToLower();
            var sourcePointer = 0;
            var targetPointer = 0;
            var previousBackIndex = -1;
            while (sourcePointer < sourceString.Length)
            {
                if (sourceString[sourcePointer] == '*')
                {
                    while (sourceString[sourcePointer] == '*')
                    {
                        sourcePointer++;
                        if (sourcePointer == sourceString.Length)
                        {
                            return true;
                        }
                    }

                    if (targetString.Length <= targetPointer)
                    {
                        return false;
                    }

                    while (targetString[targetPointer] != sourceString[sourcePointer])
                    {
                        targetPointer++;
                        if (targetPointer == targetString.Length)
                        {
                            return false;
                        }
                    }

                    previousBackIndex = sourcePointer;
                }

                if (targetPointer >= targetString.Length || sourceString[sourcePointer] != targetString[targetPointer])
                {
                    if (targetPointer < targetString.Length)
                    {
                        sourcePointer = previousBackIndex;
                    }
                    else
                    {
                        return false;
                    }
                }

                sourcePointer++;
                targetPointer++;
            }

            return targetPointer == targetString.Length;
        }

        public override string ToString() => pattern;

    }
}
