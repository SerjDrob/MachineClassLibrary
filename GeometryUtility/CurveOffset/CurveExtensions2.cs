using System;
using System.Collections.Generic;
using System.Linq;
using MachineClassLibrary.Laser.Entities;

namespace MachineClassLibrary.GeometryUtility.CurveOffset;

public static class CurveExtensions2
{
    public static IEnumerable<Curve> TestCurve(this Curve curve, double delta)
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
    public static IEnumerable<Curve> InflateCurve2(this Curve curve, double delta)
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



