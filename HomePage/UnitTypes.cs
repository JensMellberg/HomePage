namespace HomePage
{
    public static class UnitTypes
    {
        public static string[] GetUnitTypes => [Volume, Weight, Amount];

        public const string Weight = "Vikt";
        public const string Amount = "Antal";
        public const string Volume = "Volym";

        public static Dictionary<string, string[]> GetAllUnitValues()
        {
            var dict = new Dictionary<string, string[]>
            {
                { Weight, WeightInstance.AllUnitValues.Select(x => x.Unit).ToArray() },
                { Volume, VolumeInstance.AllUnitValues.Select(x => x.Unit).ToArray() },
                { Amount, AmountInstance.AllUnitValues.Select(x => x.Unit).ToArray() }
            };

            return dict;
        }

        public static UnitInstance CreateInstance(string unitType, double amount)
        {
            return unitType switch
            {
                Amount => new AmountInstance(amount),
                Weight => new WeightInstance(amount),
                Volume => new VolumeInstance(amount),
                _ => throw new Exception($"Unknown unitType {unitType}"),
            };
        }

        public static UnitInstance CreateInstance(string unitType, double amount, string unit)
        {
            var instance = CreateInstance(unitType, amount);
            instance.UpdateAmount(amount, unit);
            return instance;
        }
    }

    public abstract class UnitInstance(double amount)
    {
        private double amount = amount;

        public abstract string UnitType { get; }

        internal abstract List<UnitValue> UnitValues { get; }

        public void Multiply(double multiplier) => amount *= multiplier;

        public void UpdateAmount(double newAmount, string unit)
        {
            var multiplier = (UnitValues.FirstOrDefault(x => x.Unit.Equals(unit)) ?? UnitValues[0]).Value;
            amount = newAmount * multiplier;
        }

        public UnitInstance Combine(UnitInstance other)
        {
            amount += other.amount;
            return this;
        }

        public UnitInstance Subtract(UnitInstance other)
        {
            amount -= other.amount;
            return this;
        }

        public double Amount => amount;

        public (string amount, string unit) GetDisplayValues()
        {
            foreach (var value in UnitValues)
            {
                if (amount >= value.Value)
                {
                    var displayValue = Math.Round(amount / value.Value, 1);
                    var decimalValue = displayValue % 1;
                    if (decimalValue != 0 && decimalValue != 0.5)
                    {
                        continue;
                    }

                    return (displayValue.ToString(), value.Unit);
                }
            }

            return (amount.ToString(), UnitValues.Last().Unit);
        }
    }

    public class AmountInstance(double amount) : UnitInstance(amount)
    {
        public override string UnitType => UnitTypes.Amount;

        internal override List<UnitValue> UnitValues => AllUnitValues;

        public static List<UnitValue> AllUnitValues =>
        [
            new UnitValue { Unit = "st", Value = 1 }
        ];
    }

    public class WeightInstance(double amount) : UnitInstance(amount)
    {
        public override string UnitType => UnitTypes.Weight;

        internal override List<UnitValue> UnitValues => AllUnitValues;

        public static List<UnitValue> AllUnitValues =>
        [
            new UnitValue { Unit = "kg", Value = 1000 },
            new UnitValue { Unit = "g", Value = 1 },
        ];
    }

    public class VolumeInstance(double amount) : UnitInstance(amount)
    {
        public override string UnitType => UnitTypes.Volume;

        internal override List<UnitValue> UnitValues => AllUnitValues;

        public static List<UnitValue> AllUnitValues =>
        [
            new UnitValue { Unit = "l", Value = 1000 },
            new UnitValue { Unit = "dl", Value = 100 },
            new UnitValue { Unit = "msk", Value = 15 },
            new UnitValue { Unit = "cl", Value = 10 },
            new UnitValue { Unit = "tsk", Value = 5 },
            new UnitValue { Unit = "ml", Value = 1 },
        ];
    }

    public class UnitValue
    {
        public required string Unit { get; set; }

        public int Value { get; set; }
    }

    public class IngredientCategory(string name, int sortOrder)
    {
        public static string[] Categories => [
            Vegetables,
            Meat,
            Produce,
            Bread,
            Storage,
            Spices,
            Frozen,
            Snacks,
            Fish,
            Can,
            Pasta
        ];

        public static IngredientCategory Create(string category)
        {
            return category switch
            {
                Vegetables => new IngredientCategory(category, 1),
                Meat => new IngredientCategory(category, 20),
                Produce => new IngredientCategory(category, 30),
                Bread => new IngredientCategory(category, 10),
                Storage => new IngredientCategory(category, 50),
                Pasta => new IngredientCategory(category, 48),
                Fish => new IngredientCategory(category, 25),
                Can => new IngredientCategory(category, 55),
                Spices => new IngredientCategory(category, 40),
                Frozen => new IngredientCategory(category, 60),
                Snacks => new IngredientCategory(category, 70),
                _ => throw new Exception($"Unknown category {category}"),
            };
        }

        public const string Vegetables = "Färskvaror";
        public const string Produce = "Mejeri";
        public const string Meat = "Kött/chark";
        public const string Bread = "Bröd";
        public const string Fish = "Fisk";
        public const string Storage = "Torrvaror";
        public const string Pasta = "Pasta/ris";
        public const string Can = "Konserver";
        public const string Spices = "Smaksättning";
        public const string Frozen = "Fryst";
        public const string Snacks = "Snacks";

        public string Name { get; } = name;
        public int SortOrder { get; } = sortOrder;
    }
}
