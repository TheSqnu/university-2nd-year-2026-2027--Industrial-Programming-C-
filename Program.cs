using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneticSearch
{
    class Program
    {
        struct GeneticData
        {
            public string protein;
            public string organism;
            public string amino_acids;
        }

        struct Command
        {
            public string name;
            public string parameter1;
            public string parameter2;
        }

        static void Main(string[] args)
        {
            string sequencesFile = "sequences.0.txt";
            string commandsFile = "commands.0.txt";
            string outputFile = "genedata.0.txt";

            if (!File.Exists(sequencesFile) || !File.Exists(commandsFile))
            {
                Console.WriteLine("Ошибка: Входные файлы не найдены в папке с программой");
                return;
            }

            List<GeneticData> proteins = ReadData(sequencesFile);

            List<Command> commands = ReadCommands(commandsFile);

            CommandHandler(proteins, commands, outputFile);

            Console.WriteLine("Выполнение завершено! Результаты записаны в файл " + outputFile);
        }

        static string RLDecoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids)) return string.Empty;

            StringBuilder decoded = new StringBuilder();
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];

                if (char.IsDigit(ch))
                {
                    int count = ch - '0';
                    char letter = amino_acids[i + 1];

                    for (int j = 0; j < count; j++)
                    {
                        decoded.Append(letter);
                    }
                    i++;
                }
                else
                {
                    decoded.Append(ch);
                }
            }
            return decoded.ToString();
        }

        static List<GeneticData> ReadData(string filename)
        {
            List<GeneticData> data = new List<GeneticData>();
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    if (parts.Length >= 3)
                    {
                        GeneticData gd;
                        gd.protein = parts[0].Trim();
                        gd.organism = parts[1].Trim();
                        gd.amino_acids = RLDecoding(parts[2].Trim());
                        data.Add(gd);
                    }
                }
            }
            return data;
        }

        static List<Command> ReadCommands(string filename)
        {
            List<Command> commands = new List<Command>();
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    Command cmd;
                    cmd.name = parts[0].Trim();
                    cmd.parameter1 = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                    cmd.parameter2 = parts.Length > 2 ? parts[2].Trim() : string.Empty;
                    commands.Add(cmd);
                }
            }
            return commands;
        }

        static void CommandHandler(List<GeneticData> proteins, List<Command> commands, string outputFilename)
        {
            using (StreamWriter writer = new StreamWriter(outputFilename, false, Encoding.UTF8))
            {

                for (int i = 0; i < commands.Count; i++)
                {
                    Command cmd = commands[i];
                    string num = (i + 1).ToString("D3");

                    writer.WriteLine("--------------------------------------------------------------------------");

                    if (cmd.name == "search")
                    {
                        string searchTarget = RLDecoding(cmd.parameter1);
                        writer.WriteLine($"{num}   search   {searchTarget} ");
                        writer.WriteLine("organism\t\t\t\tprotein ");

                        bool found = false;
                        foreach (var p in proteins)
                        {
                            if (p.amino_acids.Contains(searchTarget))
                            {
                                writer.WriteLine($"{p.organism}\t\t{p.protein}");
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            writer.WriteLine("NOT FOUND");
                        }
                    }
                    else if (cmd.name == "diff")
                    {
                        writer.WriteLine($"{num}   diff   {cmd.parameter1}   {cmd.parameter2} ");
                        writer.WriteLine("amino-acids difference: ");

                        GeneticData? p1 = proteins.FirstOrDefault(p => p.protein == cmd.parameter1);
                        GeneticData? p2 = proteins.FirstOrDefault(p => p.protein == cmd.parameter2);

                        if (p1 == null || p2 == null)
                        {
                            StringBuilder missingMsg = new StringBuilder("MISSING:");
                            if (p1 == null) missingMsg.Append(" " + cmd.parameter1);
                            if (p2 == null) missingMsg.Append(" " + cmd.parameter2);
                            writer.WriteLine(missingMsg.ToString());
                        }
                        else
                        {
                            int diffCount = CalculateDiff(p1.Value.amino_acids, p2.Value.amino_acids);
                            writer.WriteLine(diffCount);
                        }
                    }
                    else if (cmd.name == "mode")
                    {
                        writer.WriteLine($"{num}   mode   {cmd.parameter1} ");
                        writer.WriteLine("amino-acid occurs:");

                        GeneticData? target = proteins.FirstOrDefault(p => p.protein == cmd.parameter1);

                        if (target == null)
                        {
                            writer.WriteLine($"MISSING: {cmd.parameter1}");
                        }
                        else
                        {
                            FindMode(target.Value.amino_acids, out char topLetter, out int maxCount);
                            writer.WriteLine($"{topLetter}          {maxCount}");
                        }
                    }
                }
                writer.WriteLine("--------------------------------------------------------------------------");
            }
        }

        static int CalculateDiff(string s1, string s2)
        {
            int diff = 0;
            int minLength = Math.Min(s1.Length, s2.Length);
            int maxLength = Math.Max(s1.Length, s2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (s1[i] != s2[i]) diff++;
            }

            diff += (maxLength - minLength);
            return diff;
        }

        static void FindMode(string seq, out char topLetter, out int maxCount)
        {
            Dictionary<char, int> counts = new Dictionary<char, int>();
            foreach (char c in seq)
            {
                if (counts.ContainsKey(c)) counts[c]++;
                else counts[c] = 1;
            }

            maxCount = 0;
            topLetter = 'A';

            foreach (var pair in counts.OrderBy(p => p.Key))
            {
                if (pair.Value > maxCount)
                {
                    maxCount = pair.Value; topLetter = pair.Key;
                }
            }
        }
    }
}