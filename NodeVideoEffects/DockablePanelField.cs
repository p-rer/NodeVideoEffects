using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NodeVideoEffects
{
    public class DockablePanelField : Panel
    {
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
            double cellWidth = finalSize.Width / 3;
            double cellHeight = finalSize.Height / 3;

            Dictionary<string, DockablePanel> panels = new();
            List<List<Cell>> cells = InitializeCells(3, 3);
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
                (int x, int y, int width, int height) = GetPanelRect(panel.Value, panel.Key);
                panel.Value.Arrange(new Rect(x * cellWidth, y * cellHeight, width * cellWidth, height * cellHeight));
            }

            return finalSize;

            // Reserve a panel at its docked position
            void ReservePanel(DockablePanel panel)
            {
                (int x, int y) = (((int)panel.DockLocation) % 3, ((int)panel.DockLocation) / 3);
                string id = Guid.NewGuid().ToString("N");
                cells[x][y] = new Cell { panelID = id, priority = panel.Priority };

                panels.Add(id, panel);
                reservedPanels.Add((x, y));
            }

            // Expand to adjacent positions while respecting constraints
            void Fill(List<(int, int)> positions, int priority)
            {
                // Directions: up, down, left, right
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
                        return; // Expand in only one direction
                    }
                }


                // Check if a position is valid for expansion
                bool IsValidPosition(int x, int y, int priority)
                {
                    return x >= 0 && y >= 0 && x < 3 && y < 3 &&
                           !reservedPanels.Contains((x, y)) &&
                           (cells[x][y] == null || cells[x][y].priority <= priority);
                }
            }

            // Get the bounding rectangle of a panel based on its occupied cells
            (int x, int y, int width, int height) GetPanelRect(DockablePanel panel, string id)
            {
                var cellsForPanel = new List<(int x, int y)>();
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

            // Initialize a 2D grid of cells
            List<List<Cell>> InitializeCells(int rows, int cols)
            {
                var grid = new List<List<Cell>>();
                for (int i = 0; i < rows; i++)
                {
                    grid.Add(new List<Cell>(new Cell[cols]));
                }
                return grid;
            }
        }

        // Represents a cell in the grid
        private class Cell
        {
            public string? panelID;
            public int priority;
        }
    }
}
