using System.Collections.Concurrent;
using static HomePage.DirectionExtensions;
using static HomePage.WordMixResultValidator;
namespace HomePage
{
    public class WordMixCalculator(IEnumerable<Letter> letters, HashSet<string> allWords, DatabaseLogger logger)
    {
        private (int score, Board board) bestBoard = (0, null);

        private HashSet<string> ValidWords = [];

        private readonly object WriteLock = new();

        private static ConcurrentDictionary<string, List<string>> WordsByPossibleLetters = [];

        private readonly object[] HashLocks =
            [.. Enumerable.Range(0, 64).Select(_ => new object())];

        private readonly HashSet<int>[] SeenBoards =
            [.. Enumerable.Range(0, 64).Select(_ => new HashSet<int>())];

        public (int score, Board board) CalculateBestBoardWithTimeout(TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            return CalculateBestBoard(cts.Token);
        }

        public (int score, Board board) CalculateBestBoard(CancellationToken? token)
        {
            SetupWords();
            var bestLetter = letters.MaxBy(x => x.Score)!;
            var startWords = ValidWords.Where(x => x.Contains(bestLetter.Character)).ToList();
            var cancellationToken = token ?? CancellationToken.None;
            var completedThreads = 0;

            try
            {
                Parallel.ForEach(
                    startWords,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
                        CancellationToken = cancellationToken
                    },
                    word =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        } 

                        var startBoard = new Board { BoardMatrix = new Letter[10, 10], UnusedLetters = letters.ToList() };
                        var startX = 5 - word.Length / 2;
                        var usedPoses = PlaceWord(startBoard, word, Direction.Right, (startX, 4)).ToList();
                        CalculatePartial(startBoard, usedPoses.Select(p => (p.x, p.y, Direction.Down)).ToList(), false, cancellationToken);
                        Interlocked.Increment(ref completedThreads);
                    });
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                logger.Log(Model.LogRowSeverity.Error, e.Message, null, e.StackTrace);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                logger.Information($"Timed out when calculating the best word mix result. {completedThreads} of {startWords.Count} start words completed.", null);
            } else
            {
                logger.Information($"Completed best word mix result calculations. A total of {startWords.Count} start words completed.", null);
            }
 
            return bestBoard;
        }

        private static (int x, int y) ApplyDelta((int x, int y) point, (int x, int y) delta, int multiplier = 1) => (point.x + delta.x * multiplier, point.y + delta.y * multiplier);

        private static (int x, int y) ApplyDelta((int x, int y) point, Direction direction, int multiplier = 1) => ApplyDelta(point, direction.GetDelta(), multiplier);

        private void CalculatePartial(Board board, IEnumerable<(int x, int y, Direction direction)> spotsToTry, bool mustSolve, CancellationToken cancellationToken)
        {
            foreach (var spot in spotsToTry)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var direction = spot.direction;
                var letter = board.BoardMatrix[spot.x, spot.y];
                var existingLetters = CollectLetters(board, direction, (spot.x, spot.y));
                var possibleWords = GetPossibleWords(board.UnusedLetters.Concat(existingLetters.Item1), ValidWords).ToList();
                var wordSubstring = string.Join("", existingLetters.Item1.Select(x => x.Character));
                var usedSpot = (existingLetters.x, existingLetters.y);
                foreach (var word in possibleWords.Where(x => x.Contains(wordSubstring)))
                {
                    var nextDirection = direction == Direction.Right ? Direction.Down : Direction.Right;
                    var positionOfLetter = word.IndexOf(wordSubstring);
                    var startPos = ApplyDelta((usedSpot.x, usedSpot.y), direction.Reverse(), positionOfLetter);
                    var usedPoses = PlaceWord(board, word, direction, startPos).ToList();

                    var validationResult = ValidateWords(board, ValidWords);
                    if (validationResult is WordMixValidationSuccess successResult)
                    {
                        if (!TryAddBoard(BoardHash(successResult.words, successResult.score)))
                        {
                            UndoWord(board, usedPoses);
                            continue;
                        }

                        if (successResult.score > bestBoard.score)
                        {
                            lock (WriteLock)
                            {
                                if (successResult.score > bestBoard.score)
                                {
                                    bestBoard = (successResult.score, Copy(board));
                                }
                            }
                        }

                        CalculatePartial(board, spotsToTry.Where(x => x != spot).Concat(usedPoses.Select(p => (p.x, p.y, nextDirection))).ToList(), false, cancellationToken);
                    } 
                    else if (validationResult is WordMixValidationError errorResult && !mustSolve)
                    {
                        if (!TryAddBoard(BoardHash(errorResult.words, null)))
                        {
                            UndoWord(board, usedPoses);
                            continue;
                        }

                        CalculatePartial(board, spotsToTry.Where(x => x != spot).Concat(usedPoses.Select(p => (p.x, p.y, nextDirection))).ToList(), true, cancellationToken);
                    }

                    UndoWord(board, usedPoses);
                }
            }
        }

        private bool TryAddBoard(int hash)
        {
            var bucket = hash & 63;
            lock (HashLocks[bucket])
            {
                return SeenBoards[bucket].Add(hash);
            }
        }

        private static void UndoWord(Board board, List<(int x, int y)> placed)
        {
            foreach (var pos in placed)
            {
                var letter = board.BoardMatrix[pos.x, pos.y];
                board.BoardMatrix[pos.x, pos.y] = null;
                board.UnusedLetters.Add(letter);
            }
        }

        private static int BoardHash(IEnumerable<string> words, int? score)
        {
            var hash = 17;
            foreach (var w in words.OrderBy(x => x))
            {
                hash = hash * 31 + w.GetHashCode();
            }
               
            if (score.HasValue)
            {
                return hash * 31 + score.Value;
            }

            return hash;
        }

        private static IEnumerable<(int x, int y)> PlaceWord(Board board, string word, Direction direction, (int x, int y) start)
        {
            var current = start;
            foreach (var c in word)
            {
                if (current.x < 0 || current.x >= board.Width || current.y < 0 || current.y >= board.Height)
                {
                    break;
                }

                if (board.BoardMatrix[current.x, current.y] == null)
                {
                    var letter = board.UnusedLetters.First(x => x.Character == c);
                    board.UnusedLetters.Remove(letter);
                    board.BoardMatrix[current.x, current.y] = letter;
                    yield return current;
                }

                current = ApplyDelta(current, direction);
            }
        }

        private static (List<Letter>, int x, int y) CollectLetters(Board board, Direction direction, (int x, int y) start)
        {
            var current = start;
            var currentDirection = direction;
            var result = new List<(Letter letter, int x, int y)>();

            while (true)
            {
                if (current.x < 0 || current.x >= board.Width || current.y < 0 || current.y >= board.Height)
                {
                    break;
                }

                var letter = board.BoardMatrix[current.x, current.y];
                if (letter != null)
                {
                    result.Add((letter, current.x, current.y));
                    current = ApplyDelta(current, currentDirection);
                } else
                {
                    break;
                }
            }

            currentDirection = currentDirection.Reverse();
            current = ApplyDelta(start, currentDirection);
            while (true)
            {
                if (current.x < 0 || current.x >= board.Width || current.y < 0 || current.y >= board.Height)
                {
                    break;
                }

                var letter = board.BoardMatrix[current.x, current.y];
                if (letter != null)
                {
                    result.Add((letter, current.x, current.y));
                    current = ApplyDelta(current, currentDirection);
                }
                else
                {
                    break;
                }
            }

            var orderedResult = result.OrderBy(x => x.x).ThenBy(x => x.y).ToList();
            return (orderedResult.Select(x => x.letter).ToList(), orderedResult[0].x, orderedResult[0].y);
        }

        private static List<string> GetPossibleWords(IEnumerable<Letter> possibleLetters, HashSet<string> validWords)
        {
            var lettersString = string.Join("", possibleLetters.Select(x => x.Character).OrderBy(x => x));
            if (WordsByPossibleLetters.TryGetValue(lettersString, out var allWords)) {
                return allWords;
            }

            var result = new List<string>();
            var letterDict = possibleLetters.ToLookup(x => x.Character);
            foreach (var word in validWords)
            {
                if (word.Length > 10 || word.Length < 2)
                {
                    continue;
                }

                var letterCounts = new Dictionary<char, int>();
                var hasFailed = false;
                foreach (var c in word)
                {
                    if (letterCounts.TryGetValue(c, out var usedCount) && usedCount == letterDict[c].Count()
                        || !letterDict.Contains(c))
                    {
                        hasFailed = true;
                        break;
                    }

                    letterCounts[c] = usedCount + 1;
                }

                if (!hasFailed)
                {
                    result.Add(word);
                }
            }

            WordsByPossibleLetters[lettersString] = result;
            return result;
        }

        private void SetupWords()
        {
            ValidWords = [.. GetPossibleWords(letters, allWords)];
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

            return new Board { BoardMatrix = otherMatrix, UnusedLetters = board.UnusedLetters.ToList() };
        }
    }

    public static class DirectionExtensions
    {

        public enum Direction
        {
            Left,
            Up,
            Down,
            Right
        }

        public static (int x, int y) GetDelta(this Direction direction) => direction switch
        {
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            _ => throw new Exception(),
        };

        public static Direction Reverse(this Direction direction) => direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw new Exception(),
        };
    }
}
