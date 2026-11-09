using System;

namespace lab5csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Candy candy1 = new Candy("Сникерс", 50, 27, 250);
            OtherSweet sweet1 = new OtherSweet("Тульский пряник", 140, 50, 400, "Пряники");

            Console.WriteLine("Проверка создания сладостей:");
            Console.WriteLine(candy1.GetFullDescription());
            Console.WriteLine(sweet1.GetFullDescription());
            
            Console.ReadLine();
        }
    }
}