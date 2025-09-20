using static HomePage.WordMixResultValidator;
namespace HomePage
{
    public class WordMixCalculator
    {
       public WordMixCalculator() { }

       private (int score, Board board) bestBoard = (0, null);

       public Board CalculateBestBoard(IEnumerable<Letter> letters)
       {
            var bestLetter = letters.MaxBy(x => x.Score);
            var start = new Letter[10, 10];
            start[4, 4] = bestLetter;
            CalculatePartial(new Board { BoardMatrix = start }, letters.Where(x => x != bestLetter), [(3, 4), (5, 4), (4, 3), (4, 5)]);

            return null;
       }

        private void CalculatePartial(Board previous, IEnumerable<Letter> remaining, List<(int x, int y)> possibleSpots)
        {
            foreach (var r in remaining)
            {
                foreach (var p in possibleSpots)
                {
                    previous.BoardMatrix[p.x, p.y] = r;

                }
            }
        }

        private static Board Copy(Board board)
        {
            var otherMatrix = new Letter[board.BoardMatrix.GetLength(0), board.BoardMatrix.GetLength(1)];
            for (var x = 0; x < otherMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < otherMatrix.GetLength(1); y++)
                {
                    if (board.BoardMatrix[x, y] != null)
                    {
                        otherMatrix[x, y] = board.BoardMatrix[x, y];
                    }
                }
            }

            return new Board { BoardMatrix = otherMatrix };
        }
    }
}
