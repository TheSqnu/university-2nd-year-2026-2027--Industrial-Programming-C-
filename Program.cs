using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string InputFile = "1.ChaseData.txt";
        string OutFile = "1.PursuitLog.txt";
        int size = 0;

        if (File.Exists(OutFile))
        {
            File.Delete(OutFile);
        }

        using (StreamReader reader = new StreamReader(InputFile))
        {
            string line = reader.ReadLine();
            if (line != null)
            {
                size = int.Parse(line.Trim());
            }
            else
            {
                Console.WriteLine("Ошибка : первая строка файла должна содержать размер поля");
                return;
            }
        }

        if (size > 0)
        {
            Game game = new Game(size);
            game.Run(InputFile, OutFile);
        }
        else
        {
            Console.WriteLine("Ошибка : некорrектный размер поля");
            return;
        }
    }
}
