using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static HomePage.WordMixResultValidator;

namespace HomePage
{
    // Public letter input type (user-provided)

    // Internal tile instance (distinct even for same character)
    public class TileInstance
    {
        public int Id { get; }
        public char Char { get; }
        public int Score { get; }
        public TileInstance(int id, char ch, int score) { Id = id; Char = ch; Score = score; }
    }

    // Cell holds a placed tile (or null)
    public class Cell
    {
        public TileInstance Tile { get; }
        public Cell(TileInstance t) { Tile = t; }
    }

    public class PlacedWord
    {
        public string Word { get; }
        public int Row { get; }
        public int Col { get; }
        public bool Horizontal { get; }
        public PlacedWord(string word, int row, int col, bool horizontal)
        {
            Word = word; Row = row; Col = col; Horizontal = horizontal;
        }
    }

    public class BoardSolution
    {
        public Cell[,] Grid { get; }
        public List<PlacedWord> PlacedWords { get; }
        public int TotalScore { get; }
        public BoardSolution(Cell[,] grid, List<PlacedWord> words, int total)
        {
            Grid = grid; PlacedWords = words; TotalScore = total;
        }

        public string PrettyPrint()
        {
            var sb = new StringBuilder();
            int size = Grid.GetLength(0);
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (Grid[r, c] == null) sb.Append(". ");
                    else sb.Append(char.ToUpperInvariant(Grid[r, c].Tile.Char) + " ");
                }
                sb.AppendLine();
            }
            sb.AppendLine($"Score: {TotalScore}");
            sb.AppendLine("Words: " + string.Join(", ", PlacedWords.Select(w => w.Word)));
            return sb.ToString();
        }
    }

    public class BestBoardFinder
    {
        private const int SIZE = 10;
        private readonly HashSet<string> _dict;
        private readonly HashSet<string> _prefixes;
        private readonly List<TileInstance> _tiles; // all tiles
        private readonly Dictionary<char, List<TileInstance>> _initialPools; // available tiles by char (ascending by score)
        private List<string> _candidates; // candidate words possible from tiles

        private BestBoardFinder(List<TileInstance> tiles, HashSet<string> dictLower)
        {
            _tiles = tiles;
            _dict = dictLower;
            _prefixes = BuildPrefixes(dictLower);
            _initialPools = BuildPools(tiles);
        }

        // Public entry: expects 13 Letter tiles and validWords (case-insensitive)
        public static BoardSolution GenerateBestBoard(List<Letter> letters, HashSet<string> validWords)
        {
            if (letters == null) throw new ArgumentNullException(nameof(letters));
            if (validWords == null) throw new ArgumentNullException(nameof(validWords));
            if (letters.Count != 13) throw new ArgumentException("Expected exactly 13 letters", nameof(letters));

            var dict = new HashSet<string>(validWords.Select(w => w.ToLowerInvariant()));
            var tiles = new List<TileInstance>();
            int id = 0;
            foreach (var L in letters)
            {
                tiles.Add(new TileInstance(id++, char.ToLowerInvariant(L.Character), L.Score));
            }

            var finder = new BestBoardFinder(tiles, dict);
            finder.BuildCandidatesFromTiles();
            return finder.Search();
        }

        #region Preparations

        private static HashSet<string> BuildPrefixes(HashSet<string> dict)
        {
            var pref = new HashSet<string>();
            foreach (var w in dict)
                for (int i = 1; i <= w.Length; i++)
                    pref.Add(w.Substring(0, i));
            return pref;
        }

        private static Dictionary<char, List<TileInstance>> BuildPools(List<TileInstance> tiles)
        {
            var d = new Dictionary<char, List<TileInstance>>();
            foreach (var t in tiles)
            {
                if (!d.ContainsKey(t.Char)) d[t.Char] = new List<TileInstance>();
                d[t.Char].Add(t);
            }
            foreach (var k in d.Keys.ToList())
                d[k] = d[k].OrderBy(t => t.Score).ToList(); // ascending by score
            return d;
        }

        // Filter dictionary to words that could be formed from the tile multiset (length >= 2)
        private void BuildCandidatesFromTiles()
        {
            var counts = _initialPools.ToDictionary(kv => kv.Key, kv => kv.Value.Count);
            _candidates = new List<string>();
            foreach (var w in _dict)
            {
                if (w.Length > _tiles.Count) continue;
                var need = new Dictionary<char, int>();
                bool ok = true;
                foreach (char ch in w)
                {
                    if (!char.IsLetter(ch)) { ok = false; break; }
                    char lc = char.ToLowerInvariant(ch);
                    if (!need.ContainsKey(lc)) need[lc] = 0;
                    need[lc]++;
                    if (!counts.ContainsKey(lc) || need[lc] > counts[lc]) { ok = false; break; }
                }
                if (ok && w.Length >= 2) _candidates.Add(w);
            }

            // Sort initially by potential (sum of top available tile scores for each letter)
            _candidates = _candidates
                .Distinct()
                .Select(w => new { Word = w, Pot = WordPotentialMax(w) })
                .OrderByDescending(x => x.Pot)
                .Select(x => x.Word)
                .ToList();
        }

        // Upper-bound potential score for a word (use highest-scoring available tiles for letters)
        private int WordPotentialMax(string w)
        {
            var poolCopy = _initialPools.ToDictionary(kv => kv.Key, kv => new Queue<TileInstance>(kv.Value.OrderByDescending(t => t.Score)));
            int sum = 0;
            foreach (char ch0 in w)
            {
                char ch = char.ToLowerInvariant(ch0);
                if (poolCopy.ContainsKey(ch) && poolCopy[ch].Count > 0)
                    sum += poolCopy[ch].Dequeue().Score;
                else sum += 1;
            }
            return sum;
        }

        #endregion

        #region Search

        private BoardSolution Search()
        {
            var grid = new Cell[SIZE, SIZE];
            var placedWords = new List<PlacedWord>();
            var pools = _initialPools.ToDictionary(kv => kv.Key, kv => new List<TileInstance>(kv.Value));
            var best = new BoardSolution(CloneGrid(grid), new List<PlacedWord>(placedWords), int.MinValue);

            // Limit branching by top K candidate words (tunable)
            int candidateLimit = Math.Min(_candidates.Count, 1200);
            var baseCandidates = _candidates.Take(candidateLimit).ToList();

            int nodes = 0;

            // dynamic overlap scoring function
            Func<string, int> overlapScore = (word) =>
            {
                int bestOverlap = 0;
                for (int r = 0; r < SIZE; r++)
                    for (int c = 0; c < SIZE; c++)
                        for (int dir = 0; dir < 2; dir++)
                        {
                            bool hor = dir == 0;
                            if ((hor && c + word.Length > SIZE) || (!hor && r + word.Length > SIZE)) continue;
                            int matches = 0;
                            bool conflict = false;
                            for (int k = 0; k < word.Length; k++)
                            {
                                int rr = r + (hor ? 0 : k), cc = c + (hor ? k : 0);
                                if (grid[rr, cc] != null)
                                {
                                    if (grid[rr, cc].Tile.Char == char.ToLowerInvariant(word[k])) matches++;
                                    else { conflict = true; break; }
                                }
                            }
                            if (!conflict) bestOverlap = Math.Max(bestOverlap, matches);
                        }
                return bestOverlap;
            };

            // Backtracking
            void Backtrack(Cell[,] g, Dictionary<char, List<TileInstance>> poolState, List<PlacedWord> pw, int currentWordsSum)
            {
                nodes++;

                // prune by optimistic bound: currentWordsSum + sum of all unused tile scores (we could at most use them)
                int unusedSum = poolState.Values.SelectMany(l => l).Sum(t => t.Score);
                int optimistic = currentWordsSum + unusedSum;
                if (optimistic <= best.TotalScore) return;

                // dynamic candidate ordering: prefer high overlap then potential
                var dynCandidates = baseCandidates
                    .Where(w => CanFormFromPools(w, poolState))
                    .Select(w => new { Word = w, Ov = overlapScore(w), Pot = WordPotentialMax(w) })
                    .OrderByDescending(x => x.Ov)
                    .ThenByDescending(x => x.Pot)
                    .Select(x => x.Word)
                    .ToList();

                foreach (var w in dynCandidates)
                {
                    int len = w.Length;
                    for (int r = 0; r < SIZE; r++)
                    {
                        for (int c = 0; c < SIZE; c++)
                        {
                            // horizontal
                            if (c + len <= SIZE)
                            {
                                if (!CanPlaceAt(g, w, r, c, true)) goto nextHorCheck; // use goto to share label to vertical
                                if (!FullAxisWordValidAfterPlacement(g, w, r, c, true)) goto nextHorCheck;
                                // determine tile instances to place for empty cells
                                var selection = ChooseTilesForPlacement(g, w, r, c, true, poolState);
                                if (selection != null)
                                {
                                    // perform placement
                                    var placedCells = new List<(int rr, int cc, TileInstance t)>();
                                    foreach (var kv in selection)
                                    {
                                        int rr = kv.Key.rr, cc = kv.Key.cc;
                                        g[rr, cc] = new Cell(kv.Value);
                                        placedCells.Add((rr, cc, kv.Value));
                                    }
                                    // remove from pools
                                    var removed = new List<TileInstance>();
                                    foreach (var t in selection.Values)
                                    {
                                        bool removedOk = poolState[t.Char].Remove(t);
                                        if (!removedOk) throw new InvalidOperationException("Tile removal mismatch");
                                        removed.Add(t);
                                    }

                                    // validate that no perpendicular or newly created words (length>=2) are invalid (this checks all sequences that include any placed cell)
                                    if (AllNewWordsValid(g, placedCells))
                                    {
                                        int addScore = ComputeWordScore(w, g, r, c, true); // this word's score (sum of tile scores in the word)
                                        pw.Add(new PlacedWord(w, r, c, true));
                                        if (AllPlacedConnected(g))
                                        {
                                            Backtrack(g, poolState, pw, currentWordsSum + addScore);
                                        }
                                        pw.RemoveAt(pw.Count - 1);
                                    }

                                    // undo
                                    foreach (var (rr, cc, t) in placedCells) g[rr, cc] = null;
                                    foreach (var t in removed) poolState[t.Char].Add(t);
                                    // keep pools ordered ascending
                                    foreach (var key in poolState.Keys.ToList())
                                        poolState[key] = poolState[key].OrderBy(tt => tt.Score).ToList();
                                }
                            }
                        nextHorCheck:
                            // vertical
                            if (r + len <= SIZE)
                            {
                                if (!CanPlaceAt(g, w, r, c, false)) continue;
                                if (!FullAxisWordValidAfterPlacement(g, w, r, c, false)) continue;
                                var selectionV = ChooseTilesForPlacement(g, w, r, c, false, poolState);
                                if (selectionV != null)
                                {
                                    var placedCells = new List<(int rr, int cc, TileInstance t)>();
                                    foreach (var kv in selectionV)
                                    {
                                        int rr = kv.Key.rr, cc = kv.Key.cc;
                                        g[rr, cc] = new Cell(kv.Value);
                                        placedCells.Add((rr, cc, kv.Value));
                                    }
                                    var removed = new List<TileInstance>();
                                    foreach (var t in selectionV.Values)
                                    {
                                        bool removedOk = poolState[t.Char].Remove(t);
                                        if (!removedOk) throw new InvalidOperationException("Tile removal mismatch");
                                        removed.Add(t);
                                    }

                                    if (AllNewWordsValid(g, placedCells))
                                    {
                                        int addScore = ComputeWordScore(w, g, r, c, false);
                                        pw.Add(new PlacedWord(w, r, c, false));
                                        if (AllPlacedConnected(g))
                                        {
                                            Backtrack(g, poolState, pw, currentWordsSum + addScore);
                                        }
                                        pw.RemoveAt(pw.Count - 1);
                                    }

                                    foreach (var (rr, cc, t) in placedCells) g[rr, cc] = null;
                                    foreach (var t in removed) poolState[t.Char].Add(t);
                                    foreach (var key in poolState.Keys.ToList())
                                        poolState[key] = poolState[key].OrderBy(tt => tt.Score).ToList();
                                }
                            }
                        }
                    }
                }

                // termination: compute total score (sum of all horizontal & vertical words length>=2) - sum(unused tile scores)
                int totalWordsScore = ComputeAllWordsScore(g);
                int penalty = poolState.Values.SelectMany(l => l).Sum(t => t.Score);
                int total = totalWordsScore - penalty;

                if (total > best.TotalScore)
                {
                    best = new BoardSolution(CloneGrid(g), new List<PlacedWord>(pw), total);
                }
            }

            Backtrack(grid, pools, placedWords, 0);
            return best;
        }

        #endregion

        #region Helpers: placement validation & tile selection

        // Quick feasibility: ensures letters match existing tiles where present
        private bool CanPlaceAt(Cell[,] g, string w, int r, int c, bool horizontal)
        {
            for (int k = 0; k < w.Length; k++)
            {
                int rr = r + (horizontal ? 0 : k), cc = c + (horizontal ? k : 0);
                char ch = char.ToLowerInvariant(w[k]);
                if (g[rr, cc] != null && g[rr, cc].Tile.Char != ch) return false;
            }
            return true;
        }

        // After placing the word, the full contiguous word along its axis (including existing letters adjacent before/after) must be a valid word (if length>=2)
        private bool FullAxisWordValidAfterPlacement(Cell[,] g, string w, int r, int c, bool horizontal)
        {
            // determine start by stepping backwards until empty or edge
            int startR = r, startC = c;
            while (true)
            {
                int br = startR - (horizontal ? 0 : 1), bc = startC - (horizontal ? 1 : 0);
                if (br < 0 || bc < 0) break;
                if (g[br, bc] == null) break;
                startR = br; startC = bc;
            }

            int endR = r + (horizontal ? 0 : w.Length - 1), endC = c + (horizontal ? w.Length - 1 : 0);
            while (true)
            {
                int fr = endR + (horizontal ? 0 : 1), fc = endC + (horizontal ? 1 : 0);
                if (fr >= SIZE || fc >= SIZE) break;
                if (g[fr, fc] == null) break;
                endR = fr; endC = fc;
            }

            // Build the full string (inject placed word's chars where needed)
            var sb = new StringBuilder();
            int rr = startR, cc = startC;
            while (true)
            {
                if (horizontal)
                {
                    if (rr == r && cc >= c && cc < c + w.Length)
                    {
                        sb.Append(char.ToLowerInvariant(w[cc - c]));
                    }
                    else if (g[rr, cc] != null)
                    {
                        sb.Append(g[rr, cc].Tile.Char);
                    }
                    else break;

                    if (cc == endC) break;
                    cc++;
                }
                else
                {
                    if (cc == c && rr >= r && rr < r + w.Length)
                    {
                        sb.Append(char.ToLowerInvariant(w[rr - r]));
                    }
                    else if (g[rr, cc] != null)
                    {
                        sb.Append(g[rr, cc].Tile.Char);
                    }
                    else break;

                    if (rr == endR) break;
                    rr++;
                }
            }

            string full = sb.ToString();
            if (full.Length == 0) return false;
            if (full.Length == 1) return true; // single-letter vertical/horizontal sequences allowed but do not count as words
            return _dict.Contains(full);
        }

        // After placing tiles, any perpendicular contiguous sequences of length >= 2 that include any newly placed cell must be valid words
        // placedCells: cells placed by the attempted placement (so we only need to validate sequences touching those coordinates)
        private bool AllNewWordsValid(Cell[,] g, List<(int rr, int cc, TileInstance t)> placedCells)
        {
            foreach (var (rr, cc, t) in placedCells)
            {
                // check perpendicular axis relative to the word that placed this cell: test both axes (safe)
                // horizontal perpendicular: vertical word at (rr,cc)
                string vert = BuildContiguousWord(g, rr, cc, vertical: true);
                if (vert.Length >= 2 && !_dict.Contains(vert)) return false;
                string hor = BuildContiguousWord(g, rr, cc, vertical: false);
                if (hor.Length >= 2 && !_dict.Contains(hor)) return false;
            }
            return true;
        }

        // Build contiguous string along axis (vertical true => up/down; false => left/right) that includes cell (r,c)
        private string BuildContiguousWord(Cell[,] g, int r, int c, bool vertical)
        {
            int startR = r, startC = c;
            while (true)
            {
                int br = startR - (vertical ? 1 : 0), bc = startC - (vertical ? 0 : 1);
                if (br < 0 || bc < 0) break;
                if (g[br, bc] == null) break;
                startR = br; startC = bc;
            }
            int endR = r, endC = c;
            while (true)
            {
                int fr = endR + (vertical ? 1 : 0), fc = endC + (vertical ? 0 : 1);
                if (fr >= SIZE || fc >= SIZE) break;
                if (g[fr, fc] == null) break;
                endR = fr; endC = fc;
            }

            var sb = new StringBuilder();
            int rr = startR, cc = startC;
            while (true)
            {
                sb.Append(g[rr, cc].Tile.Char);
                if (rr == endR && cc == endC) break;
                if (vertical) rr++; else cc++;
            }
            return sb.ToString();
        }

        // Choose specific tile instances for empty cells of a candidate placement.
        // Heuristic: if cell will be crossed (has neighbor in perpendicular direction), pick highest score available; else pick lowest.
        // Returns mapping from coordinates to chosen TileInstance, or null if not feasible.
        private Dictionary<(int rr, int cc), TileInstance> ChooseTilesForPlacement(Cell[,] g, string w, int r, int c, bool horizontal, Dictionary<char, List<TileInstance>> pools)
        {
            var selection = new Dictionary<(int rr, int cc), TileInstance>();
            for (int k = 0; k < w.Length; k++)
            {
                int rr = r + (horizontal ? 0 : k), cc = c + (horizontal ? k : 0);
                char needed = char.ToLowerInvariant(w[k]);

                if (g[rr, cc] != null) continue; // existing tile used

                if (!pools.ContainsKey(needed) || pools[needed].Count == 0) return null;

                bool willBeCross = false;
                if (horizontal)
                {
                    if ((rr - 1 >= 0 && g[rr - 1, cc] != null) || (rr + 1 < SIZE && g[rr + 1, cc] != null)) willBeCross = true;
                }
                else
                {
                    if ((cc - 1 >= 0 && g[rr, cc - 1] != null) || (cc + 1 < SIZE && g[rr, cc + 1] != null)) willBeCross = true;
                }

                TileInstance pick;
                var poolList = pools[needed];
                if (willBeCross)
                {
                    pick = poolList.OrderByDescending(t => t.Score).FirstOrDefault();
                }
                else
                {
                    pick = poolList.OrderBy(t => t.Score).FirstOrDefault();
                }
                if (pick == null) return null;

                // ensure we don't pick the same tile twice for two different spots in this placement
                if (selection.Values.Any(tt => tt.Id == pick.Id))
                {
                    var alt = willBeCross ? poolList.OrderByDescending(t => t.Score).FirstOrDefault(tt => !selection.Values.Any(v => v.Id == tt.Id))
                                          : poolList.OrderBy(t => t.Score).FirstOrDefault(tt => !selection.Values.Any(v => v.Id == tt.Id));
                    if (alt == null) return null;
                    pick = alt;
                }

                selection[(rr, cc)] = pick;
            }
            return selection;
        }

        // Validate that all placed tiles (entire board) are connected (single connected component)
        private bool AllPlacedConnected(Cell[,] g)
        {
            (int, int) first = (-1, -1);
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    if (g[r, c] != null) { first = (r, c); goto found; }
                found:
            if (first.Item1 == -1) return true;
            var visited = new bool[SIZE, SIZE];
            var q = new Queue<(int r, int c)>();
            q.Enqueue(first);
            visited[first.Item1, first.Item2] = true;
            int cnt = 0;
            while (q.Count > 0)
            {
                var (rr, cc) = q.Dequeue();
                cnt++;
                foreach (var (nr, nc) in Neigh4(rr, cc))
                {
                    if (nr < 0 || nc < 0 || nr >= SIZE || nc >= SIZE) continue;
                    if (visited[nr, nc]) continue;
                    if (g[nr, nc] == null) continue;
                    visited[nr, nc] = true;
                    q.Enqueue((nr, nc));
                }
            }
            return cnt == CountPlaced(g);
        }

        private static IEnumerable<(int r, int c)> Neigh4(int r, int c)
        {
            yield return (r - 1, c);
            yield return (r + 1, c);
            yield return (r, c - 1);
            yield return (r, c + 1);
        }

        private int CountPlaced(Cell[,] g)
        {
            int s = 0;
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    if (g[r, c] != null) s++;
            return s;
        }

        // Check pool feasibility
        private bool CanFormFromPools(string w, Dictionary<char, List<TileInstance>> pools)
        {
            var need = new Dictionary<char, int>();
            foreach (char ch0 in w)
            {
                char ch = char.ToLowerInvariant(ch0);
                if (!need.ContainsKey(ch)) need[ch] = 0;
                need[ch]++;
                if (!pools.ContainsKey(ch) || need[ch] > pools[ch].Count) return false;
            }
            return true;
        }

        #endregion

        #region Scoring utilities

        // Compute score for a single word placed at (r,c) along orientation (assumes all tiles present in grid)
        private int ComputeWordScore(string word, Cell[,] g, int r, int c, bool horizontal)
        {
            int s = 0;
            for (int k = 0; k < word.Length; k++)
            {
                int rr = r + (horizontal ? 0 : k), cc = c + (horizontal ? k : 0);
                s += g[rr, cc].Tile.Score;
            }
            return s;
        }

        // Compute total score: sum of all horizontal and vertical contiguous sequences >=2 (each counts separately)
        private int ComputeAllWordsScore(Cell[,] g)
        {
            int total = 0;
            // horizontal
            for (int r = 0; r < SIZE; r++)
            {
                int c = 0;
                while (c < SIZE)
                {
                    if (g[r, c] == null) { c++; continue; }
                    int start = c;
                    int sum = 0;
                    while (c < SIZE && g[r, c] != null)
                    {
                        sum += g[r, c].Tile.Score;
                        c++;
                    }
                    int len = c - start;
                    if (len >= 2) total += sum;
                }
            }
            // vertical
            for (int c = 0; c < SIZE; c++)
            {
                int r = 0;
                while (r < SIZE)
                {
                    if (g[r, c] == null) { r++; continue; }
                    int start = r;
                    int sum = 0;
                    while (r < SIZE && g[r, c] != null)
                    {
                        sum += g[r, c].Tile.Score;
                        r++;
                    }
                    int len = r - start;
                    if (len >= 2) total += sum;
                }
            }
            return total;
        }

        #endregion

        #region Utilities

        private static Cell[,] CloneGrid(Cell[,] g)
        {
            int n = g.GetLength(0), m = g.GetLength(1);
            var ng = new Cell[n, m];
            for (int r = 0; r < n; r++)
                for (int c = 0; c < m; c++)
                    ng[r, c] = g[r, c] == null ? null : new Cell(g[r, c].Tile);
            return ng;
        }

        #endregion
    }

    /* Example usage (uncomment into Main for quick test):

    class Program
    {
        static void Main()
        {
            var letters = new List<Letter>
            {
                new Letter{Character='E', Score=1}, new Letter{Character='E', Score=3},
                new Letter{Character='T', Score=1}, new Letter{Character='A', Score=2},
                new Letter{Character='R', Score=1}, new Letter{Character='S', Score=2},
                new Letter{Character='L', Score=1}, new Letter{Character='N', Score=1},
                new Letter{Character='I', Score=1}, new Letter{Character='O', Score=1},
                new Letter{Character='B', Score=3}, new Letter{Character='D', Score=2},
                new Letter{Character='M', Score=4}
            };

            var dict = new HashSet<string>(new[] {
                "rate","rates","eat","tea","rain","train","brain","node","done","stone","tones",
                "in","on","at","an","man","ram","bar","lab","lined","linedo" });

            var solution = BestBoardFinder.GenerateBestBoard(letters, dict);
            Console.WriteLine(solution.PrettyPrint());
        }
    }

    */

}
