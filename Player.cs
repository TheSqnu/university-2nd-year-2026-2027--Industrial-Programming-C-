using System;

enum State
{
    Winner,
    Looser,
    Playing,
    NotInGame
}

class Player
{
    public string name;
    public int location;
    public State state = State.NotInGame;
    public int distanceTraveled = 0;
    public bool IsStartLocation = true;

    public Player(string name)
    {
        this.name = name;
        this.location = 0;
    }

    public void Move(int step, int size)
    {
        location += step;
        distanceTraveled += Math.Abs(step);


        while (location > size)
        {
            location -= size;
        }
        while (location <= 0)
        {
            location += size;
        }
    }
}
