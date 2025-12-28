using System;
public static class RandomNumber
{
    // Random Seed
    static ulong _seed = 0;
    static ulong _counter = 0;
    static ulong[] _memory;
    static public ulong GetSeed() { return _seed; }

    static public void Init()
    {
        //Debug.Log(DateTime.Now);
        _seed = (ulong)DateTime.Now.Ticks / (ulong)TimeSpan.TicksPerMillisecond;
        MemorySetup();
    }

    static public void Init(ulong seed)
    {
        //Given a seed
        _seed = seed;
        MemorySetup();
    }

    static public void Init(bool customSeed, ulong seed)
    {
        //Given a seed
        if (customSeed)
        {
            Init(seed);
        }
        else
        {
            Init();
        }
    }



    static private void MemorySetup()
    {
        _counter = 0;
        _memory = new ulong[5];
        _memory[0] = _seed;
    }

    static public ulong Next()
    {
        ulong t = _memory[4];
        ulong s = _memory[0];
        _memory[4] = _memory[3];
        _memory[3] = _memory[2];
        _memory[2] = _memory[1];
        _memory[1] = s;

        t ^= t >> 2;
        t ^= t << 1;
        t ^= s ^ (s << 4);
        _memory[0] = t;
        _counter += 362437;

        return t + _counter;
    }

    static public ulong NextInRange(ulong min, ulong max)
    {
        if(min >= max) { return min; }
        ulong random = Next();
        random = random % (max - min);
        return random + min;
    }

    static public ulong NextMax(ulong max)
    {
        if(max == 0) { return 0; }
        ulong random = Next();
        return random % max;
    }
}
