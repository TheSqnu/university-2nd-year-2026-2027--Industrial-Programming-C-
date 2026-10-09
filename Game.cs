using System;
using System.IO;
using System.Text;

enum GameState
{
    Start,
    End
}

class Game
{
    public int size;
    public Player cat;
    public Player mouse;
    public GameState state;
    private StringBuilder result;

    public Game(int size)
    {
        this.size = size;
        cat = new Player("Cat");
        mouse = new Player("Mouse");
        state = GameState.Start;
        result = new StringBuilder();
    }

    public void Run(string InputFile, string OutFile)
    {
        result.AppendLine("Cat and Mouse");
        result.AppendLine();
        result.AppendLine("Cat Mouse  Distance");
        result.AppendLine("-------------------");

        using (StreamReader reader = new StreamReader(InputFile))
        {
            reader.ReadLine();

            while (state != GameState.End && !reader.EndOfStream)
            {
                string line = reader.ReadLine();

                char foundChar = '\0';
                int foundInt = 0;

                string digitsOnly = "";
                foreach (char ch in line)
                {
                    if (char.IsDigit(ch) || ch == '-')
                    {
                        digitsOnly += ch;
                    }
                }

                if (!string.IsNullOrEmpty(digitsOnly))
                {
                    int.TryParse(digitsOnly, out foundInt);
                }

                foreach (char ch in line)
                {
                    if (!char.IsDigit(ch) && !char.IsWhiteSpace(ch) && ch != '-')
                    {
                        foundChar = ch;
                        break;
                    }
                }

                if (foundChar == 'P')
                {
                    DoPrintCommand();
                }
                else if (foundChar == 'M' || foundChar == 'C')
                {
                    DoMoveCommand(foundChar, foundInt);
                }
            }

            if (state != GameState.End && cat.location != mouse.location)
            {
                cat.state = State.Looser;
                mouse.state = State.Winner;
                state = GameState.End;
                DoPrintCommand();
            }
        }

        File.WriteAllText(OutFile, result.ToString());
    }

    private void DoMoveCommand(char foundChar, int foundInt)
    {
        switch (foundChar)
        {
            case 'M':
                if (mouse.IsStartLocation)
                {
                    mouse.location = foundInt;
                    mouse.IsStartLocation = false;
                    mouse.state = State.Playing;
                }
                else
                {
                    mouse.Move(foundInt, size);
                }
                break;

            case 'C':
                if (cat.IsStartLocation)
                {
                    cat.location = foundInt;
                    cat.IsStartLocation = false;
                    cat.state = State.Playing;
                }
                else
                {
                    cat.Move(foundInt, size);
                }
                break;
        }

        if (cat.state != State.NotInGame && mouse.state != State.NotInGame && cat.location == mouse.location)
        {
            cat.state = State.Winner;
            mouse.state = State.Looser;
            state = GameState.End;
            DoPrintCommand();
        }
    }

    private void DoPrintCommand()
    {
        result.Append(cat.state == State.NotInGame ? " ??" : $"{cat.location,3}");
        result.Append(mouse.state == State.NotInGame ? "    ??" : $"{mouse.location,6}");
        result.AppendLine(cat.state == State.NotInGame || mouse.state == State.NotInGame ? "" : $"{GetDistance(cat, mouse),10}");

        if (state == GameState.End)
        {
            result.AppendLine("-------------------");
            result.AppendLine();
            result.AppendLine();
            result.AppendLine("Пройденное рассттояние:  Мышь  Кот");
            result.AppendLine($"{mouse.distanceTraveled,27} {cat.distanceTraveled,6}");
            result.AppendLine();

            if (mouse.state == State.Looser && cat.state == State.Winner)
            {
                result.AppendLine($"Мышь была поймана на : {cat.location}");
            }
            else if (mouse.state == State.Winner && cat.state == State.Looser)
            {
                result.AppendLine("Мышь убежала от кота");
            }
        }
    }

    private int GetDistance(Player cat, Player mouse)
    {
       return Math.Abs(cat.location - mouse.location);
    }
}
