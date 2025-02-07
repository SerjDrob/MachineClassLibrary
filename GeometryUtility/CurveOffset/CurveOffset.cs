using System;
using System.Collections.Generic;
using System.Linq;
using MachineClassLibrary.Laser.Entities;

namespace MachineClassLibrary.GeometryUtility.CurveOffset;

public static class CurveExtensions
{
    public static IEnumerable<Curve> InflateCurve(this Curve curve, double delta)
    {
        var vertices = curve.Vertices.ToList();
        var newVertices = new List<(double X, double Y, double Bulge)>();

        // Смещаем вершины
        for (int i = 0; i < vertices.Count; i++)
        {
            var (x1, y1, bulge1) = vertices[i];
            var (x2, y2, bulge2) = vertices[(i + 1) % vertices.Count];

            if (bulge1 == 0)
            {
                // Линейный сегмент
                double dx = x2 - x1;
                double dy = y2 - y1;
                double length = Math.Sqrt(dx * dx + dy * dy);
                double nx = -dy / length; // Нормаль к направлению
                double ny = dx / length;

                // Смещаем вершину на расстояние delta
                double offsetX1 = x1 + nx * delta;
                double offsetY1 = y1 + ny * delta;
                double offsetX2 = x2 + nx * delta;
                double offsetY2 = y2 + ny * delta;

                newVertices.Add((offsetX1, offsetY1, 0));
                newVertices.Add((offsetX2, offsetY2, 0));
            }
            else
            {
                // Дуга (обработка выпуклости)
                var arcPoints = ApproximateArc(x1, y1, x2, y2, bulge1, delta);
                newVertices.AddRange(arcPoints);
            }
        }

        // Проверка на самопересечения
        var intersections = BentleyOttmann.FindIntersections(newVertices);
        if (intersections.Any())
        {
            // Разделяем кривую на части
            var curves = SplitCurveAtIntersections(newVertices, intersections);
            foreach (var c in curves)
            {
                yield return new Curve(c, curve.IsClosed);
            }
        }
        else
        {
            yield return new Curve(newVertices, curve.IsClosed);
        }
    }

    private static List<(double X, double Y, double Bulge)> ApproximateArc(double x1, double y1, double x2, double y2, double bulge, double delta)
    {
        var points = new List<(double X, double Y, double Bulge)>();
        double chordLength = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        double radius = (chordLength / 2) * (1 + Math.Pow(bulge, 2)) / (4 * bulge);
        double centerAngle = 4 * Math.Atan(Math.Abs(bulge));

        // Вычисляем центр дуги
        double midX = (x1 + x2) / 2;
        double midY = (y1 + y2) / 2;
        double dx = x2 - x1;
        double dy = y2 - y1;
        double perpendicularX = -dy;
        double perpendicularY = dx;
        double length = Math.Sqrt(perpendicularX * perpendicularX + perpendicularY * perpendicularY);
        perpendicularX /= length;
        perpendicularY /= length;

        double centerX = midX + perpendicularX * (radius - delta);
        double centerY = midY + perpendicularY * (radius - delta);

        // Аппроксимируем дугу
        int steps = 10; // Количество шагов для аппроксимации
        for (int i = 0; i <= steps; i++)
        {
            double angle = centerAngle * i / steps;
            double x = centerX + radius * Math.Cos(angle);
            double y = centerY + radius * Math.Sin(angle);
            points.Add((x, y, 0));
        }

        return points;
    }

    private static IEnumerable<List<(double X, double Y, double Bulge)>> SplitCurveAtIntersections(List<(double X, double Y, double Bulge)> vertices, List<(int, int, (double X, double Y))> intersections)
    {
        var graph = new Dictionary<(double X, double Y), List<(double X, double Y)>>();

        // Строим граф
        for (int i = 0; i < vertices.Count - 1; i++)
        {
            var p1 = (vertices[i].X, vertices[i].Y);
            var p2 = (vertices[i + 1].X, vertices[i + 1].Y);

            if (!graph.ContainsKey(p1)) graph[p1] = new List<(double X, double Y)>();
            if (!graph.ContainsKey(p2)) graph[p2] = new List<(double X, double Y)>();

            graph[p1].Add(p2);
            graph[p2].Add(p1);
        }

        // Добавляем точки пересечения в граф
        foreach (var (i, j, intersectionPoint) in intersections)
        {
            if (!graph.ContainsKey(intersectionPoint)) graph[intersectionPoint] = new List<(double X, double Y)>();

            var p1 = (vertices[i].X, vertices[i].Y);
            var p2 = (vertices[i + 1].X, vertices[i + 1].Y);
            var p3 = (vertices[j].X, vertices[j].Y);
            var p4 = (vertices[j + 1].X, vertices[j + 1].Y);

            graph[p1].Add(intersectionPoint);
            graph[intersectionPoint].Add(p1);
            graph[p2].Add(intersectionPoint);
            graph[intersectionPoint].Add(p2);
            graph[p3].Add(intersectionPoint);
            graph[intersectionPoint].Add(p3);
            graph[p4].Add(intersectionPoint);
            graph[intersectionPoint].Add(p4);
        }

        // Обход графа для выделения контуров
        var visited = new HashSet<(double X, double Y)>();
        foreach (var point in graph.Keys)
        {
            if (!visited.Contains(point))
            {
                var contour = new List<(double X, double Y)>();
                DFS(point, graph, visited, contour);
                yield return contour.Select(p => (p.X, p.Y, 0.0)).ToList();
            }
        }
    }
    private static void DFS((double X, double Y) point, Dictionary<(double X, double Y), List<(double X, double Y)>> graph, HashSet<(double X, double Y)> visited, List<(double X, double Y)> contour)
    {
        visited.Add(point);
        contour.Add(point);

        foreach (var neighbor in graph[point])
        {
            if (!visited.Contains(neighbor))
            {
                DFS(neighbor, graph, visited, contour);
            }
        }
    }
}

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
            return segA.Y.CompareTo(segB.Y);
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
                var above = activeSegments.GetViewBetween(currentEvent.SegmentIndex + 1, int.MaxValue).Min;
                var below = activeSegments.GetViewBetween(int.MinValue, currentEvent.SegmentIndex - 1).Max;

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



