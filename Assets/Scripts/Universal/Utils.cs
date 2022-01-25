using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static Sprite Texture2DToSprite(this Texture2D texture)
    {
        return Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    public static Point Point2fToPoint(Point2f p2f) => new Point(((int)p2f.X), ((int)p2f.Y));

    public static List<T> ShiftLeft<T>(this List<T> list, int shiftBy)
    {
        if (list.Count <= shiftBy)
            return list;

        var result = list.GetRange(shiftBy, list.Count - shiftBy);
        result.AddRange(list.GetRange(0, shiftBy));
        return result;
    }

    public static List<Point2f> Sort4Points2fClockwiseFromTop(List<Point2f> points)
    {
        var distMin = points.Min(p => Math.Pow(p.X, 2) + Math.Pow(p.Y, 2));

        Point2f firstPoint = (from p in points
                              where Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) == distMin
                              select p).FirstOrDefault();

        var avgPoint = new Point2f(points.Average(t => t.X), points.Average(t => t.Y));
        points = points.OrderBy(p => Math.Atan2(avgPoint.Y - p.Y, avgPoint.X - p.X)).ToList();
        return points.ShiftLeft(points.IndexOf(firstPoint));
    }

    public static List<Point> Sort4PointsClockwiseFromTop(List<Point> points)
    {
        var distMin = points.Min(p => Math.Pow(p.X, 2) + Math.Pow(p.Y, 2));

        Point firstPoint = (from p in points
                              where Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) == distMin
                              select p).FirstOrDefault();

        var avgPoint = new Point(points.Average(t => t.X), points.Average(t => t.Y));
        points = points.OrderBy(p => Math.Atan2(avgPoint.Y - p.Y, avgPoint.X - p.X)).ToList();
        return points.ShiftLeft(points.IndexOf(firstPoint));
    }

    public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}


