using System;

namespace lab5csharp
{
    public abstract class Sweet
    {
        public string Name;
        public double Weight;
        public int sugar;
        public int calories;

        public Sweet(string Name, double Weight, int sugar, int calories)
        {
            this.Name = Name;
            this.Weight = Weight;
            this.sugar = sugar;
            this.calories = calories;
        }

        public abstract string GetFullDescription();

        public (double weight, int sugar, int calories) GetSortingParams()
        {
            return (Weight, sugar, calories);
        }

        public abstract string GetBriefInfo();
    }

    public class Candy : Sweet
    {
        public Candy(string name, double weight, int sugar, int calories) : base(name, weight, sugar, calories) { } 
        
        public override string GetFullDescription()
        {
            return $"=> Конфета «{Name}» | Масса: {Weight} г | Энергетическая ценность: {calories} ккал | Сахар: {sugar} г";
        }

        public override string GetBriefInfo()
        {
            return $"- {Name} ({Weight} г)\n";
        }
    }

    public class OtherSweet : Sweet
    {
        string Category;
        public OtherSweet(string name, double weight, int sugar, int calories, string Category) : base(name, weight, sugar, calories) { this.Category = Category; }

        public override string GetFullDescription()
        {
            return $"=> Сладость «{Name}» [{Category}] | Масса: {Weight} г | Энергетическая ценность: {calories} ккал | Сахар: {sugar} г";
        }

        public override string GetBriefInfo()
        {
            return $"- {Name} [{Category}] ({Weight} г)\n";
        }
    }
}