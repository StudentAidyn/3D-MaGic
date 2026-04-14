using System;

public static class TimeKeeper
{
    // DEBUG / PROCESS TIMING VARIABLES AND CONTROLS **********************************************
    // ********************************************************************************************

    static private DateTime s_start;
    static private DateTime s_end;

    public static void RegisterStartTime()
    {
        s_start = DateTime.Now;
    }
    public static void RegisterEndTime()
    {
        s_end = DateTime.Now;
    }

    public static TimeSpan GetTotalTime()
    {
        return (s_end - s_start);
    }

    // ********************************************************************************************
    // ********************************************************************************************
}
