using System.Collections.Generic;

namespace MachineClassLibrary.GeometryUtility.CurveOffset;

public static class BentleyOttmann
{
    public static List<(int, int, (double X, double Y))> FindIntersections(List<(double X, double Y, double Bulge)> vertices)
    {
        var intersections = new List<(int, int, (double X, double Y))>();
        var events = new SortedSet<Event>(Comparer<Event>.Create((a, b) => a.X.CompareTo(b.X)));

        // Добавляем события для всех отрезков
        for (int i = 0; i < vertices.Count - 1; i++)
        {
            var p1 = vertices[i];
            var p2 = vertices[i + 1];
            if (p1.X < p2.X)
            {
                events.Add(new Event(p1.X, p1.Y, EventType.Start, i));
                events.Add(new Event(p2.X, p2.Y, EventType.End, i));
            }
            else
            {
                events.Add(new Event(p2.X, p2.Y, EventType.Start, i));
                events.Add(new Event(p1.X, p1.Y, EventType.End, i));
            }
        }

        var activeSegments = new SortedSet<int>(Comparer<int>.Create((a, b) =>
        {
            var segA = vertices[a];
            var segB = vertices[b];
            var result = segA.Y.CompareTo(segB.Y);
            return result;
        }));

        while (events.Count > 0)
        {
            var currentEvent = events.Min;
            events.Remove(currentEvent);

            if (currentEvent.Type == EventType.Start)
            {
                // Добавляем отрезок в активные
                activeSegments.Add(currentEvent.SegmentIndex);

                // Проверяем пересечения с соседями
                //var above = activeSegments.GetViewBetween(currentEvent.SegmentIndex + 1, int.MaxValue).Min;
                //var below = activeSegments.GetViewBetween(int.MinValue, currentEvent.SegmentIndex - 1).Max;
                var onlyOne = activeSegments.Count == 1;
                var above = onlyOne ? default(int) : activeSegments.GetViewBetween(currentEvent.SegmentIndex + 1, activeSegments.Count - 1).Min;
                var below = onlyOne ? default(int) : activeSegments.GetViewBetween(0, currentEvent.SegmentIndex - 1).Max;

                if (above != default)
                {
                    var intersection = FindIntersectionPoint(
                        (vertices[currentEvent.SegmentIndex].X, vertices[currentEvent.SegmentIndex].Y),
                        (vertices[currentEvent.SegmentIndex + 1].X, vertices[currentEvent.SegmentIndex + 1].Y),
                        (vertices[above].X, vertices[above].Y),
                        (vertices[above + 1].X, vertices[above + 1].Y));

                    if (intersection != null)
                    {
                        intersections.Add((currentEvent.SegmentIndex, above, intersection.Value));
                    }
                }

                if (below != default)
                {
                    var intersection = FindIntersectionPoint(
                        (vertices[currentEvent.SegmentIndex].X, vertices[currentEvent.SegmentIndex].Y),
                        (vertices[currentEvent.SegmentIndex + 1].X, vertices[currentEvent.SegmentIndex + 1].Y),
                        (vertices[below].X, vertices[below].Y),
                        (vertices[below + 1].X, vertices[below + 1].Y));

                    if (intersection != null)
                    {
                        intersections.Add((currentEvent.SegmentIndex, below, intersection.Value));
                    }
                }
            }
            else if (currentEvent.Type == EventType.End)
            {
                // Удаляем отрезок из активных
                activeSegments.Remove(currentEvent.SegmentIndex);
            }
        }

        return intersections;
    }

    private static (double X, double Y)? FindIntersectionPoint(
    (double X, double Y) p1, (double X, double Y) p2,
    (double X, double Y) p3, (double X, double Y) p4)
    {
        double x1 = p1.X, y1 = p1.Y;
        double x2 = p2.X, y2 = p2.Y;
        double x3 = p3.X, y3 = p3.Y;
        double x4 = p4.X, y4 = p4.Y;

        double denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        // Если отрезки параллельны
        if (denominator == 0)
            return null;

        double t1 = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
        double t2 = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denominator;

        // Если точка пересечения лежит на обоих отрезках
        if (t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1)
        {
            double x = x1 + t1 * (x2 - x1);
            double y = y1 + t1 * (y2 - y1);
            return (x, y);
        }

        return null;
    }
    private enum EventType { Start, End }

    private class Event
    {
        public double X { get; }
        public double Y { get; }
        public EventType Type { get; }
        public int SegmentIndex { get; }

        public Event(double x, double y, EventType type, int segmentIndex)
        {
            X = x;
            Y = y;
            Type = type;
            SegmentIndex = segmentIndex;
        }
    }
}



