using System.Text;
using HomePage.Data;
using HomePage.Model;

namespace HomePage.Repositories
{
    public class CurrentWordMixRepository(AppDbContext dbContext)
    {
        public CurrentWordMix GetCurrent()
        {
            var current = dbContext.CurrentWordMix.Single();
            //WordList();
            if (current.ShouldRecreate)
            {
                Recreate(current);
            }

            return current;
        }

        public void WordList()
        {
            var words = File.ReadAllLines("Database/WordList - Original.txt");
            var keptWords = new List<string>();
            HashSet<char> vowels = ['A', 'E', 'I', 'U', 'O', 'Ä', 'Y', 'Ö'];
            MultisidedDie[] dies = [
                new((7, 'C'), (1, 'S'), (3, 'Ä'), (1, 'U'), (1, 'L'), (2, 'O')),
                new((1, 'L'), (2, 'O'), (1, 'K'), (3, 'P'), (3, 'Y'), (2, 'N')),
                new((1, 'L'), (5, 'D'), (1, 'I'), (2, 'O'), (2, 'R'), (1, 'A')),
                new((1, 'A'), (3, 'V'), (1, 'S'), (7, 'X'), (2, 'H'), (1, 'I')),
                new((7, 'Ä')/*,Joker*/, (1, 'E'), (2, 'N'), (2, 'O'), (1, 'K')),
                new((2, 'M'), (1, 'T'), (1, 'U'), (1, 'L'), (1, 'E'), (2, 'J')),
                new((1, 'A'), (2, 'R'), (1, 'S'), (1, 'U'), (3, 'Y'), (5, 'D')),
                new((3, 'P'), (1, 'T'), (1, 'E'), (1, 'S'), (4, 'G'), (5, 'F')),
                new((2, 'M'), (1, 'U'), (1, 'T'), (2, 'H'), (3, 'V'), (1, 'A')),
                new((2, 'J'), (2, 'N'), (1, 'K'), (1, 'S'), (1, 'A'), (3, 'P')),
                new((3, 'Ä'), (1, 'E'), (1, 'T'), (1, 'K'), (6, 'B'), (1, 'I')),
                new((1, 'K'), (3, 'Ä'), (1, 'I'), (2, 'R'), (2, 'H'), (2, 'N')),
                new((5, 'Ö'), (4, 'G'), (2, 'M'), (3, 'V'), (1, 'T'), (1, 'A')),
            ];

            var letters = dies.SelectMany(x => x.sides.Select(t => t.letter)).Distinct().ToHashSet();
            var excluded = File.ReadAllLines("Database/ExcludedWords.txt").Select(x => x.ToUpper()).ToHashSet();
            foreach (var word in words)
            {
                var invalid = false;
                var hasVowel = false;
                if (word == word.ToUpper() || excluded.Contains(word.ToUpper()))
                {
                    continue;
                }

                foreach (var c in word.ToUpper())
                {
                    if (!letters.Contains(c))
                    {
                        invalid = true;
                        break;
                    }

                    if (vowels.Contains(c))
                    {
                        hasVowel = true;
                    }
                }

                if (!invalid && word.Length < 14 && hasVowel)
                {
                    var canMake = CanMakeWord(word.ToUpper(), dies, 0);
                    if (canMake)
                    {
                        keptWords.Add(word);
                    }
                }
            }

            var sb = new StringBuilder();
            sb.Append("var dict = {\n");
            foreach (var w in keptWords)
            {
                sb.Append("\"");
                sb.Append(w);
                sb.Append("\": true,\n");
            }

            sb.Append("}");
            File.WriteAllText("Database/WordList.txt", sb.ToString());
        }

        private bool CanMakeWord(string word, IEnumerable<MultisidedDie> dice, int letterIndex)
        {
            if (letterIndex == word.Length)
            {
                return true;
            }

            var makers = dice.Where(x => x.sides.Any(s => s.letter == word[letterIndex])).ToList();
            foreach (var die in makers)
            {
                if (CanMakeWord(word, dice.Where(x => x != die), letterIndex + 1))
                {
                    return true;
                }
            }

            return false;
        }

        public void Recreate(CurrentWordMix entity)
        {
            MultisidedDie[] dies = [
                new((7, 'C'), (1, 'S'), (3, 'Ä'), (1, 'U'), (1, 'L'), (2, 'O')),
                new((1, 'L'), (2, 'O'), (1, 'K'), (3, 'P'), (3, 'Y'), (2, 'N')),
                new((1, 'L'), (5, 'D'), (1, 'I'), (2, 'O'), (2, 'R'), (1, 'A')),
                new((1, 'A'), (3, 'V'), (1, 'S'), (7, 'X'), (2, 'H'), (1, 'I')),
                new((7, 'Ä')/*,Joker*/, (1, 'E'), (2, 'N'), (2, 'O'), (1, 'K')),
                new((2, 'M'), (1, 'T'), (1, 'U'), (1, 'L'), (1, 'E'), (2, 'J')),
                new((1, 'A'), (2, 'R'), (1, 'S'), (1, 'U'), (3, 'Y'), (5, 'D')),
                new((3, 'P'), (1, 'T'), (1, 'E'), (1, 'S'), (4, 'G'), (5, 'F')),
                new((2, 'M'), (1, 'U'), (1, 'T'), (2, 'H'), (3, 'V'), (1, 'A')),
                new((2, 'J'), (2, 'N'), (1, 'K'), (1, 'S'), (1, 'A'), (3, 'P')),
                new((3, 'Ä'), (1, 'E'), (1, 'T'), (1, 'K'), (6, 'B'), (1, 'I')),
                new((1, 'K'), (3, 'Ä'), (1, 'I'), (2, 'R'), (2, 'H'), (2, 'N')),
                new((5, 'Ö'), (4, 'G'), (2, 'M'), (3, 'V'), (1, 'T'), (1, 'A')),
            ];
            HashSet<char> vowels = ['A', 'E', 'I', 'U', 'O', 'Ä', 'Y', 'Ö'];
            var dieResult = ThrowDice();
            entity.Letters = string.Join("", dieResult.Select(x => x.letter.ToString() + x.value));
            entity.CreatedAt = DateHelper.DateTimeNow;
            dbContext.CurrentWordMix.Update(entity);
            dbContext.SaveChanges();

            List<(int value, char letter)> ThrowDice()
            {
                var result = dies.Select(d => d.ThrowDie()).ToList();
                if (result.Count(x => vowels.Contains(x.letter)) < 2)
                {
                    return ThrowDice();
                }

                return result;
            }
        }

        private class MultisidedDie
        {
            public (int value, char letter)[] sides;
            public MultisidedDie(params (int value, char letter)[] sides)
            {
                this.sides = sides;
            }

            public (int value, char letter) ThrowDie() => sides[Random.Shared.Next(0, sides.Length)];
        }
    }
}
