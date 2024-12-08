using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NodeVideoEffects
{
    public class DockablePanelField : Panel
    {
        // Define the row and column ratios
        public List<double> RowRatios { get; set; } = new() { 1, 1, 1 };
        public List<double> ColumnRatios { get; set; } = new() { 1, 1, 1 };

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // Normalize ratios to ensure they sum to 1
            double totalRowRatio = NormalizeRatios(RowRatios);
            double totalColumnRatio = NormalizeRatios(ColumnRatios);

            // Calculate the actual cell widths and heights
            var cellWidths = CalculateCellSizes(finalSize.Width, ColumnRatios, totalColumnRatio);
            var cellHeights = CalculateCellSizes(finalSize.Height, RowRatios, totalRowRatio);

            Dictionary<string, DockablePanel> panels = new();
            List<List<Cell>> cells = InitializeCells(RowRatios.Count, ColumnRatios.Count);
            List<(int, int)> reservedPanels = new();

            // Reserve panels
            foreach (UIElement child in InternalChildren)
            {
                if (child is not DockablePanel panel)
                    continue;
                ReservePanel(panel);
            }

            // Fill unoccupied spaces
            foreach ((int, int) pos in reservedPanels)
            {
                Fill(new List<(int, int)> { pos }, cells[pos.Item1][pos.Item2].priority);
            }

            // Arrange panels in their calculated positions
            foreach (var panel in panels)
            {
                (int _x, int _y, int _width, int _height) = GetPanelRect(panel.Key);
                double left = SumSizes(cellWidths, 0, _x);
                double top = SumSizes(cellHeights, 0, _y);
                double width = SumSizes(cellWidths, _x, _width);
                double height = SumSizes(cellHeights, _y, _height);

                panel.Value.Arrange(new Rect(left, top, width, height));
            }

            return finalSize;

            // Reserve a panel at its docked position
            void ReservePanel(DockablePanel panel)
            {
                (int x, int y) = (((int)panel.DockLocation) % ColumnRatios.Count, ((int)panel.DockLocation) / ColumnRatios.Count);
                string id = Guid.NewGuid().ToString("N");
                cells[x][y] = new Cell { panelID = id, priority = panel.Priority };

                panels.Add(id, panel);
                reservedPanels.Add((x, y));
            }

            // Expand to adjacent positions while respecting constraints
            void Fill(List<(int, int)> positions, int priority)
            {
                (sbyte dx, sbyte dy)[] directions = [(0, 1), (0, -1), (1, 0), (-1, 0)];

                foreach (var (dx, dy) in directions)
                {
                    List<(int, int)> newPositions = new(positions);
                    bool canExpand = true;

                    foreach (var (x, y) in positions)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (!positions.Contains((nx, ny)))
                        {
                            if (!IsValidPosition(nx, ny, priority))
                            {
                                canExpand = false;
                                break;
                            }
                            newPositions.Add((nx, ny));
                        }
                    }

                    if (canExpand)
                    {
                        foreach (var (nx, ny) in newPositions)
                        {
                            cells[nx][ny] = new Cell { panelID = cells[positions[0].Item1][positions[0].Item2].panelID, priority = priority };
                        }
                        Fill(newPositions, priority);
                        return;
                    }
                }

                bool IsValidPosition(int x, int y, int priority)
                {
                    return x >= 0 && y >= 0 && x < RowRatios.Count && y < ColumnRatios.Count &&
                           !reservedPanels.Contains((x, y)) &&
                           (cells[x][y] == null || cells[x][y].priority <= priority);
                }
            }

            (int x, int y, int width, int height) GetPanelRect(string id)
            {
                List<(int x, int y)> cellsForPanel = new();
                for (int i = 0; i < cells.Count; i++)
                {
                    for (int j = 0; j < cells[i].Count; j++)
                    {
                        if (cells[i][j]?.panelID == id)
                        {
                            cellsForPanel.Add((i, j));
                        }
                    }
                }

                int minX = int.MaxValue, minY = int.MaxValue;
                int maxX = int.MinValue, maxY = int.MinValue;

                foreach (var (x, y) in cellsForPanel)
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }

                return (minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
        }

        private List<List<Cell>> InitializeCells(int rows, int cols)
        {
            var grid = new List<List<Cell>>();
            for (int i = 0; i < rows; i++)
            {
                grid.Add(new List<Cell>(new Cell[cols]));
            }
            return grid;
        }

        private double NormalizeRatios(List<double> ratios)
        {
            double sum = 0;
            foreach (var ratio in ratios)
                sum += ratio;

            for (int i = 0; i < ratios.Count; i++)
                ratios[i] /= sum;

            return sum;
        }

        private List<double> CalculateCellSizes(double totalSize, List<double> ratios, double totalRatio)
        {
            var sizes = new List<double>();
            foreach (var ratio in ratios)
                sizes.Add(totalSize * ratio / totalRatio);
            return sizes;
        }

        private double SumSizes(List<double> sizes, int start, int count)
        {
            double sum = 0;
            for (int i = start; i < start + count; i++)
                sum += sizes[i];
            return sum;
        }

        private class Cell
        {
            public string? panelID;
            public int priority;
        }
    }
}
