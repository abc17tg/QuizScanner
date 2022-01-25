using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lines
{
    private List<LineSegmentPoint> m_lines;
    private List<LineSegmentPoint> m_verticalLines;
    private List<LineSegmentPoint> m_horizontalLines;
    private List<Point> m_intersections;

    public List<LineSegmentPoint> AllLines => m_lines;
    public List<LineSegmentPoint> VerticalLines => m_verticalLines;
    public List<LineSegmentPoint> HorizontalLines => m_horizontalLines;
    public List<Point> Intersections => m_intersections;

    public void SetLines(LineSegmentPoint[] lines, Size matSize)
    {
        m_lines = lines.ToList();
        m_lines.Add(new LineSegmentPoint(new Point(1, 1), new Point(matSize.Width - 1, 1)));
        m_lines.Add(new LineSegmentPoint(new Point(1, 1), new Point(1, matSize.Height - 1)));
        m_lines.Add(new LineSegmentPoint(new Point(1, matSize.Height - 1), new Point(matSize.Width - 1, matSize.Height - 1)));
        m_lines.Add(new LineSegmentPoint(new Point(matSize.Width - 1, 1), new Point(1, matSize.Height - 1)));
    }

    public void FindIntesections(Size matSize, Table table, Mat mainMat)
    {
        GetSplittedLines();
        RemoveCloseLines(matSize, table);
        ExtendLines(matSize);

        m_intersections = new List<Point>();
        for (int j = 0; j < m_verticalLines.Count; j++)
            for (int i = 0; i < m_horizontalLines.Count; i++)
                if (LineSegmentPoint.IntersectedSegments(m_verticalLines[j], m_horizontalLines[i]))
                    m_intersections.Add(LineSegmentPoint.LineIntersection(m_verticalLines[j], m_horizontalLines[i]).GetValueOrDefault());

        m_lines = m_verticalLines;
        m_lines.AddRange(m_horizontalLines);
        m_intersections = m_intersections.OrderBy(p => p.Y).ThenBy(p => p.X).ToList();
    }

    private void ExtendLines(Size matSize)
    {
        int distance = Mathf.RoundToInt(matSize.Width / 2);
        do
        {
            for (int i = 0; i < m_horizontalLines.Count; i++)
                m_horizontalLines[i] = new LineSegmentPoint(new Point(Mathf.Clamp(m_horizontalLines[i].P1.X - distance, 0, matSize.Width), Mathf.Clamp(m_horizontalLines[i].P1.Y, 0, matSize.Height)),
                    new Point(Mathf.Clamp(m_horizontalLines[i].P2.X + distance, 0, matSize.Width), Mathf.Clamp(m_horizontalLines[i].P2.Y, 0, matSize.Height)));
        } while (m_horizontalLines.Any(p => p.P1.X > 0 || p.P2.X < matSize.Width));

        do
        {
            for (int i = 0; i < m_verticalLines.Count; i++)
                m_verticalLines[i] = new LineSegmentPoint(new Point(Mathf.Clamp(m_verticalLines[i].P1.X, 0, matSize.Width), Mathf.Clamp(m_verticalLines[i].P1.Y + distance, 0, matSize.Height)),
                    new Point(Mathf.Clamp(m_verticalLines[i].P2.X, 0, matSize.Width), Mathf.Clamp(m_verticalLines[i].P2.Y - distance, 0, matSize.Height)));
        } while (m_verticalLines.Any(p => p.P2.Y > 0 || p.P1.Y < matSize.Height));
    }

    private void GetSplittedLines()
    {
        m_verticalLines = new List<LineSegmentPoint>();
        m_horizontalLines = new List<LineSegmentPoint>();
        for (int i = 0; i < m_lines.Count; i++)
            if (Mathf.Abs(m_lines[i].P1.X - m_lines[i].P2.X) < 20 && Mathf.Abs(m_lines[i].P1.Y - m_lines[i].P2.Y) > 20)
                m_verticalLines.Add(m_lines[i]);
            else if (Mathf.Abs(m_lines[i].P1.X - m_lines[i].P2.X) > 20 && Mathf.Abs(m_lines[i].P1.Y - m_lines[i].P2.Y) < 20)
                m_horizontalLines.Add(m_lines[i]);

        m_verticalLines = m_verticalLines.Distinct().OrderBy(p => p.P1.X).ToList();
        m_horizontalLines = m_horizontalLines.Distinct().OrderBy(p => p.P1.Y).ToList();
    }

    private void RemoveCloseLines(Size matSize, Table table)
    {
        int minDistInPixX = Mathf.RoundToInt((float)matSize.Width / table.Columns * 0.6f);
        int minDistInPixY = Mathf.RoundToInt((float)matSize.Height / table.Rows * 0.45f);
        List<LineSegmentPoint> filteredLines = new List<LineSegmentPoint>();

        var lines = m_verticalLines.OrderByDescending(p => Point.DistancePow2(p.P1, p.P2)).ToList();
        filteredLines.Add(lines[0]);
        for (int i = 1; i < lines.Count; i++)
        {
            if (filteredLines.All(p => Mathf.Abs(p.P1.X - lines[i].P1.X) > minDistInPixX))
                filteredLines.Add(lines[i]);
            if (filteredLines.Count == table.Columns + 1)
                break;
        }
        m_verticalLines = filteredLines.OrderBy(p => p.P1.X).ToList();

        filteredLines.Clear();
        lines = m_horizontalLines.OrderByDescending(p => Point.DistancePow2(p.P1, p.P2)).ToList();
        filteredLines.Add(lines[0]);
        for (int i = 1; i < lines.Count; i++)
        {
            if (filteredLines.All(p => Mathf.Abs(p.P1.Y - lines[i].P1.Y) > minDistInPixY))
                filteredLines.Add(lines[i]);
            if (filteredLines.Count == table.Rows + 1)
                break;
        }
        m_horizontalLines = filteredLines.OrderBy(p => p.P1.Y).ToList();
    }
}