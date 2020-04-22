using System;
using System.ComponentModel;
using UnityEngine;

// ReSharper disable CheckNamespace

/// <summary>
/// Contains utility methods that manage general "Incremental Game" functionality,
/// such as calculating the number of "grabs" that have happened in a given time period.
///
/// Because this is a "Utility" class, methods should be <c>static</c> whenever possible.
/// </summary>
public class IncrementalUtils
{
    /// <summary>
    /// Returns a new int whose value is the number of grabs that occurred during the given duration. Discards leftover progression to next grab
    /// </summary>
    /// <param name="grabInterval">Time in Ticks needed to elapse in order to get one grab</param>
    /// <param name="duration">Time in Ticks that has elapsed</param>
    /// <returns>Returns number of grabs</returns>
    public static int NumberOfGrabs(long grabInterval, long duration)
    {
        return Convert.ToInt32(duration / grabInterval);
    }

    /// <summary>
    /// Returns a new int whose value is the number of grabs that occurred during the given times. Discards leftover progression to next grab
    /// </summary>
    /// <param name="grabInterval">Time in Ticks needed to elapse in order to get one grab</param>
    /// <param name="startTime">Time in Ticks at which the duration started</param>
    /// <param name="endTime">Time in Ticks at which the duration ended</param>
    /// <returns>Returns number of grabs</returns>
    public static int NumberOfGrabs(long grabInterval, long startTime, long endTime)
    {
        if(startTime > endTime)
            Debug.LogError("endTime is earlier than startTime!");
        return NumberOfGrabs(grabInterval, (endTime - startTime));
    }

    /// <summary>
    /// Returns a new Long whose value is a modulo of the duration to the grabInterval 
    /// </summary>
    /// <param name="grabInterval">Time in Ticks needed to elapse in order to get one grab</param>
    /// <param name="duration"> Time in Ticks that has elapsed</param>
    /// <returns>Returns number of ticks elapsed towards the next grab</returns>
    public static long ProgressOfGrab(long grabInterval, long duration)
    {
        return (duration % grabInterval);
    }
    
    /// <summary>
    /// Returns a new Long whose value is a modulo of the difference of the given times to the grabInterval
    /// </summary>
    /// <param name="grabInterval">Time in Ticks needed to elapse in order to get one grab</param>
    /// <param name="startTime">Time in Ticks at which the duration started</param>
    /// <param name="endTime">Time in Ticks at which the duration ended</param>
    /// <returns>Returns number of ticks elapsed towards the next grab</returns>
    public static long ProgressOfGrab(long grabInterval, long startTime, long endTime)
    {
        if(startTime > endTime)
            Debug.LogError("endTime is earlier than startTime!");
        return ProgressOfGrab(grabInterval, (endTime - startTime));
    }

    /// <summary>
    /// Returns a new Long whose value is a summation of the current progress and a given duration
    /// </summary>
    /// <param name="currentProgress">Time in Ticks that have already elapsed</param>
    /// <param name="duration">Time in Ticks that will become considered as elapsed</param>
    /// <returns>Returns updated Ticks</returns>
    public static long ProgressUpdate(long currentProgress, long duration)
    {
        return currentProgress + duration;
    }
    
    /// <summary>
    /// Returns a new Long whose value is a summation of
    /// </summary>
    /// <param name="currentProgress">Time in Ticks that have already elapsed</param>
    /// <param name="startTime">Time in Ticks at which the duration started</param>
    /// <param name="endTime">Time in Ticks at which the duration ended</param>
    /// <returns>Returns updated Ticks</returns>
    public static long ProgressUpdate(long currentProgress, long startTime, long endTime)
    {
        return ProgressUpdate(currentProgress, (endTime - startTime));
    }
    
    
}