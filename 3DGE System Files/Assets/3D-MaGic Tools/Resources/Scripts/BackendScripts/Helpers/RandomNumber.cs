using System;
public static class RandomNumber
{
    // Random Seed
    static private ulong s_seed = 0;
    static private ulong s_counter = 0;
    static private ulong[] s_memory;
    static public ulong GetSeed() { return s_seed; }

    static public void Init()
    {
        //Debug.Log(DateTime.Now);
        s_seed = (ulong)DateTime.Now.Ticks / (ulong)TimeSpan.TicksPerMillisecond;
        MemorySetup();
    }

    static public void Init(ulong seed)
    {
        //Given a seed
        s_seed = seed;
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
        s_counter = 0;
        s_memory = new ulong[5];
        s_memory[0] = s_seed;
    }

    static public ulong Next()
    {
        ulong t = s_memory[4];
        ulong s = s_memory[0];
        s_memory[4] = s_memory[3];
        s_memory[3] = s_memory[2];
        s_memory[2] = s_memory[1];
        s_memory[1] = s;

        t ^= t >> 2;
        t ^= t << 1;
        t ^= s ^ (s << 4);
        s_memory[0] = t;
        s_counter += 362437;

        return t + s_counter;
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
