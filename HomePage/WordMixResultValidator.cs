using System.Text;
using HomePage.Data;
using static HomePage.DirectionExtensions;

namespace HomePage
{
    public record WordMixValidationResult(List<string> words);

    public record WordMixValidationError(string message, List<(int x, int y, Direction direction)> errorWords, List<string> words) : WordMixValidationResult(words);

    public record WordMixValidationSuccess(int score, List<string> words): WordMixValidationResult(words);

    public static class WordMixResultValidator
    {
        public static HashSet<string> GetAllWords(AppDbContext dbContext)
        {
            var result = new HashSet<string>();
            var lines = File.ReadAllLines("wwwroot/js/WordList.js");
            foreach (var line in lines)
            {
                if (line[0] == '\"')
                {
                    var sb = new StringBuilder();
                    sb.Append(line[1]);
                    var index = 2;
                    while (line[index] != '\"')
                    {
                        sb.Append(line[index]);
                        index++;
                    }

                    result.Add(sb.ToString().ToUpper());
                }
            }

            var extraWords = dbContext.ExtraWord.ToList();
            foreach (var word in extraWords.Where(x => x.JensApproved && x.AnnaApproved))
            {
                result.Add(word.Word.ToUpper());
            }

            return result;
        }

        public static WordMixValidationResult ValidateResult(string boardString, string availableLettersString, AppDbContext dbContext)
        {
            var availableLetters = ParseAvailableLetters(availableLettersString);
            var board = Board.ParseFromString(boardString);
            if (!ValidateLetters(board.AllLetters, availableLetters))
            {
                return new WordMixValidationError("The letters in the board does not match the available letters", [], []);
            }

            if (!ValidateConnectivity(board))
            {
                return new WordMixValidationError("Not all board letters are connected.", [], []);
            }

            return ValidateWords(board, dbContext);
        }

        public static WordMixValidationResult ValidateWords(Board board, AppDbContext dbContext) => ValidateWords(board, GetAllWords(dbContext));

        public static WordMixValidationResult ValidateWords(Board board, HashSet<string> allWords)
        {
            var totalScore = 0;
            string? error = null;
            var errorWords = new List<(int x, int y, Direction direction)>();
            var words = new List<string>();
            Iterate(board.BoardMatrix.GetLength(1), board.BoardMatrix.GetLength(0), (o, i) => board.BoardMatrix[i, o]);
            errorWords = [.. errorWords.Select(t => (t.y, t.x, Direction.Right))];
            Iterate(board.BoardMatrix.GetLength(0), board.BoardMatrix.GetLength(1), (o, i) => board.BoardMatrix[o, i]);
            
            totalScore -= board.UnusedLetters.Sum(x => x.Score);
            return error != null ? new WordMixValidationError(error, errorWords, words) : new WordMixValidationSuccess(totalScore, words);

            void Iterate(int outerMax, int innerMax, Func<int, int, Letter> getLetter)
            {
                for (var outer = 0; outer < outerMax; outer++)
                {
                    for (var inner = 0; inner < innerMax; inner++)
                    {
                        var letter = getLetter(outer, inner);
                        if (letter != null)
                        {
                            var innerStart = inner;
                            var word = letter.Character.ToString();
                            var score = letter.Score;
                            inner++;
                            while (inner < innerMax && getLetter(outer, inner) != null)
                            {
                                letter = getLetter(outer, inner);
                                word += letter.Character;
                                score += letter.Score;
                                inner++;
                            }

                            if (word.Length > 1)
                            {
                                words.Add(word);
                                if (allWords.Contains(word))
                                {
                                    totalScore += score;
                                }
                                else
                                {
                                    error = $"{word} is not a valid word.";
                                    errorWords.Add((outer, innerStart, Direction.Down));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool ValidateConnectivity(Board board)
        {
            var visited = new HashSet<(int x, int y)>();
            var needsToVisit = new HashSet<(int x, int y)>();
            var queue = new Queue<(int x, int y)>();
            for (var x = 0; x < board.BoardMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < board.BoardMatrix.GetLength(1); y++)
                {
                    if (board.BoardMatrix[x, y] != null)
                    {
                        needsToVisit.Add((x, y));
                        if (!queue.Any())
                        {
                            queue.Enqueue((x, y));
                        }
                    }
                }
            }
            
            while (queue.Any())
            {
                (var x, var y) = queue.Dequeue();
                visited.Add((x, y));
                needsToVisit.Remove((x, y));

                TryEnqueue(x - 1, y);
                TryEnqueue(x + 1, y);
                TryEnqueue(x, y - 1);
                TryEnqueue(x, y + 1);
            }

            return !needsToVisit.Any();

            void TryEnqueue(int x, int y)
            {
                if (x < 0 || y < 0 || x >= board.BoardMatrix.GetLength(0) || y >= board.BoardMatrix.GetLength(1)
                    || visited.Contains((x, y)) || board.BoardMatrix[x, y] ==  null)
                {
                    return;
                }

                queue.Enqueue((x, y));
            }
        }

        private static bool ValidateLetters(IEnumerable<Letter> boardLetters, List<Letter> availableLetters)
        {
            var lettersToBeUsed = availableLetters.ToList();
            foreach (var letter in boardLetters)
            {
                var copy = lettersToBeUsed.Find(l => l.Equals(letter));
                if (copy == null)
                {
                    return false;
                }

                lettersToBeUsed.Remove(copy);
            }

            return !lettersToBeUsed.Any();
        }

        private static List<Letter> ParseAvailableLetters(string availableLetters)
        {
            var result = new List<Letter>();
            for (var i = 0; i < availableLetters.Length; i+=2)
            {
                result.Add(new Letter { Character = availableLetters[i], Score = int.Parse(availableLetters[i + 1].ToString()) });
            }

            return result;
        }

        public class Board
        {
            public List<Letter> UnusedLetters { get; set; } = [];

            public Letter[,] BoardMatrix { get; set; }

            public int Width => BoardMatrix.GetLength(0);

            public int Height => BoardMatrix.GetLength(1);

            public IEnumerable<Letter> AllLetters
            {
                get
                {
                    for (var x = 0; x < Width; x++)
                    {
                        for (var y = 0; y < Height; y++)
                        {
                            if (BoardMatrix[x, y] != null)
                            {
                                yield return BoardMatrix[x, y];
                            }
                        }
                    }

                    foreach (var l in UnusedLetters)
                    {
                        yield return l;
                    }
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        var letter = BoardMatrix[x, y];
                        if (letter != null)
                        {
                            sb.Append($"{x},{y},{letter.Score},{letter.Character};");
                        }
                    }
                }

                foreach (var letter in UnusedLetters)
                {
                    sb.Append($"-,-,{letter.Score},{letter.Character};");
                }

                return sb.ToString();
            }

            public static Board ParseFromString(string boardString)
            {
                var result = new Board();
                var letterCoords = new Dictionary<(int x, int y), Letter>();
                foreach (var token in boardString.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var tokens = token.Split(',');
                    var letter = new Letter { Score = int.Parse(tokens[2]), Character = tokens[3].Single() };
                    if (tokens[0] == "-")
                    {
                        result.UnusedLetters.Add(letter);
                    } else
                    {
                        letterCoords.Add((int.Parse(tokens[0]), int.Parse(tokens[1])), letter);
                    }
                }

                var yMax = letterCoords.Keys.Select(x => x.y).Max();
                var xMax = letterCoords.Keys.Select(x => x.x).Max();
                result.BoardMatrix = new Letter[xMax + 1, yMax + 1];
                foreach (var kv in letterCoords)
                {
                    result.BoardMatrix[kv.Key.x, kv.Key.y] = kv.Value;
                }

                return result;
            }
        }
        
        public class Letter
        {
            public char Character { get; set; }

            public int Score { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj is  Letter otherLetter)
                {
                    return otherLetter.Character.Equals(Character) && otherLetter.Score.Equals(Score);
                }

                return base.Equals(obj);
            }

            public static List<Letter> ParseAvailableLetters(string availableLetters)
            {
                var result = new List<Letter>();
                for (var i = 0; i < availableLetters.Length; i += 2)
                {
                    result.Add(new Letter { Character = availableLetters[i], Score = int.Parse(availableLetters[i + 1].ToString()) });
                }

                return result;
            }
        }
    }
}
