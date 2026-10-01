using System;
using System.IO;

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
    public bool IsStart = true;
    public int cat_step = 0;
    public int mouse_step = 0;

    public Game(int size)
    {
        this.size = size;
        cat = new Player("Cat");
        mouse = new Player("Mouse");
        state = GameState.Start;
    }

    public void Run(string InputFile, string OutFile)
    {
        using (StreamReader reader = new StreamReader(InputFile))
        {
            reader.ReadLine();

            while (state != GameState.End && !reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

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

                if (foundChar == 'C')
                {
                    if (cat.IsStartLocation == true)
                    {
                        cat.location = foundInt;
                        cat.IsStartLocation = false;
                        cat.state = State.Playing;
                        continue;
                    }
                    cat_step += foundInt;
                }

                if (foundChar == 'M')
                {
                    if (mouse.IsStartLocation ==true)
                    {
                        mouse.location = foundInt;
                        mouse.IsStartLocation = false;
                        mouse.state = State.Playing;
                        continue;
                    }
                    mouse_step += foundInt;
                }

                if (foundChar == 'P')
                {
                    cat.Move(cat_step, size);
                    mouse.Move(mouse_step, size);

                    cat_step = 0;
                    mouse_step = 0;

                    if (cat.location == mouse.location)
                    {
                        cat.state = State.Winner;
                        mouse.state = State.Looser;
                        state = GameState.End;
                    }

                    DoMoveCommand(OutFile);
                }
            }

            if (state != GameState.End && cat.location != mouse.location)
            {
                cat.state = State.Looser;
                mouse.state = State.Winner;
                state = GameState.End;
                DoMoveCommand(OutFile);
            }
        }
    }

    private void DoMoveCommand(string OutFile)
    {
        using (StreamWriter writer = new StreamWriter(OutFile, true))
        {
            if (IsStart)
            {
                writer.WriteLine("Cat and Mouse");
                writer.WriteLine();
                writer.WriteLine("Cat Mouse  Distance");
                writer.WriteLine("-------------------");
                IsStart = false;
            }

            if (cat.state == State.NotInGame)
            {
                writer.Write(" ??");
            }
            else
            {
                writer.Write("{0,3}", cat.location);
            }

            if (mouse.state == State.NotInGame)
            {
                writer.Write("    ??");
            }
            else
            {
                writer.Write("{0,6}", mouse.location);
            }

            if (cat.state == State.NotInGame || mouse.state == State.NotInGame)
            {
                writer.WriteLine();
            }
            else
            {
                writer.WriteLine("{0,10}", GetDistance(cat, mouse));
            }

            if (state == GameState.End)
            {
                writer.WriteLine("-------------------");
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("Расстояние пройдено:   Mouse    Cat");
                writer.WriteLine("{0,27} {1,6}", mouse.distanceTraveled, cat.distanceTraveled);
                writer.WriteLine();

                if (mouse.state == State.Looser && cat.state == State.Winner)
                {
                    writer.WriteLine("Мышь была поймана на позиции: {0}", cat.location);
                }
                else if (mouse.state == State.Winner && cat.state == State.Looser)
                {
                    writer.WriteLine("Мышь убежала от кота");
                }
            }
        }
    }


    private int GetDistance(Player cat, Player mouse)
    {
        return Math.Abs(cat.location - mouse.location);
    }
}
