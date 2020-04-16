using System;
// ReSharper disable CheckNamespace

/// <summary>
/// Contains utility methods that manage general "Incremental Game" functionality,
/// such as calculating the number of "grabs" that have happened in a given time period.
///
/// Because this is a "Utility" class, methods should be <c>static</c> whenever possible.
/// </summary>
public class IncrementalUtils
{
    public static int NumberOfGrabs(float grabInterval, float duration)
    {
        return Convert.ToInt32(Math.Floor(duration / grabInterval));
    }

    public static int NumberOfGrabs(float grabInterval, long startTime, long endTime)
    {
        return NumberOfGrabs(grabInterval, (endTime - startTime));
    }

    public static float ProgressOfGrabs(float grabInterval, float duration)
    {
        return (duration % grabInterval);
    }
    
    public static float ProgressOfGrabs(float grabInterval, float startTime, float endTime)
    {
        return ProgressOfGrabs(grabInterval, (endTime - startTime));
    }
}