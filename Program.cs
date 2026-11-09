using System;

namespace lab5csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите название подарка:");
            Gift gift = new Gift(Console.ReadLine());

            gift.AddDirectly(new Candy("Сникерс", 50, 27, 250));
            gift.AddDirectly(new Candy("Красная Шапочка", 15, 10, 80));
            gift.AddDirectly(new OtherSweet("Тульский пряник", 140, 50, 400, "Пряники"));

            while (true)
            {
                int choice = ShowMenu();
                switch (choice)
                {
                    case 1:
                        gift.AddNewSweet();
                        break;
                    case 2:
                        gift.CalculateTotalWeight();
                        break;
                    case 3:
                        gift.SortSweets();
                        break;
                    case 4:
                        gift.FindBySugar();
                        break;
                    case 5:
                        Console.WriteLine(gift.ShowGiftComposition());
                        break;
                    default: return;
                }
            }
        }

        public static int ShowMenu()
        {
            Console.Write("""

                1.Добавить сладость
                2.Подсчитать вес подарка
                3.Сортировка по параметрам
                4.Поиск по сахару
                5.Инфо подарка
                Ввод: 
                """);
            return int.Parse(Console.ReadLine());
        }
    }
}