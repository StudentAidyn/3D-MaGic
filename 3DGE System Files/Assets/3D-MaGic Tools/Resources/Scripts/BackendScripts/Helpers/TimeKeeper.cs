using System;

public static class TimeKeeper
{
    // DEBUG / PROCESS TIMING VARIABLES AND CONTROLS **********************************************
    // ********************************************************************************************

    static DateTime _dt_start;
    static DateTime _dt_end;

    public static void RegisterStartTime()
    {
        _dt_start = DateTime.Now;
    }
    public static void RegisterEndTime()
    {
        _dt_end = DateTime.Now;
    }

    public static TimeSpan GetTotalTime()
    {
        return (_dt_end - _dt_start);
    }

    // ********************************************************************************************
    // ********************************************************************************************
}
