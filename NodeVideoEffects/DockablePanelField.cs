using System.Windows;
using System.Windows.Controls;

namespace NodeVideoEffects;

public class DockablePanelField : Panel
{
    // Define the row and column ratios
    private List<double> RowRatios { get; } = [1, 1, 1];
    private List<double> ColumnRatios { get; } = [1, 1, 1];

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (UIElement child in InternalChildren) child.Measure(availableSize);
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Normalize ratios to ensure they sum to 1
        var totalRowRatio = NormalizeRatios(RowRatios);
        var totalColumnRatio = NormalizeRatios(ColumnRatios);

        // Calculate the actual cell widths and heights
        var cellWidths = CalculateCellSizes(finalSize.Width, ColumnRatios, totalColumnRatio);
        var cellHeights = CalculateCellSizes(finalSize.Height, RowRatios, totalRowRatio);

        Dictionary<string, DockablePanel> panels = new();
        var cells = InitializeCells(RowRatios.Count, ColumnRatios.Count);
        List<(int, int)> reservedPanels = [];

        // Reserve panels
        foreach (UIElement child in InternalChildren)
        {
            if (child is not DockablePanel panel)
                continue;
            ReservePanel(panel);
        }

        // Fill unoccupied spaces
        foreach (var pos in reservedPanels) Fill([pos], cells[pos.Item1][pos.Item2]!.Priority);

        // Arrange panels in their calculated positions
        foreach (var panel in panels)
        {
            var (x, y, columnSpan, rowSpan) = GetPanelRect(panel.Key);
            var left = SumSizes(cellWidths, 0, x);
            var top = SumSizes(cellHeights, 0, y);
            var width = SumSizes(cellWidths, x, columnSpan);
            var height = SumSizes(cellHeights, y, rowSpan);

            panel.Value.Arrange(new Rect(left, top, width, height));
        }

        return finalSize;

        // Reserve a panel at its docked position
        void ReservePanel(DockablePanel panel)
        {
            var (x, y) = ((int)panel.DockLocation % ColumnRatios.Count, (int)panel.DockLocation / ColumnRatios.Count);
            var id = Guid.NewGuid().ToString("N");
            cells[x][y] = new Cell { PanelId = id, Priority = panel.Priority };

            panels.Add(id, panel);
            reservedPanels.Add((x, y));
        }

        // Expand to adjacent positions while respecting constraints
        void Fill(List<(int, int)> positions, int priority)
        {
            (sbyte dx, sbyte dy)[] directions = [(0, 1), (0, -1), (1, 0), (-1, 0)];

            foreach (var (dx, dy) in directions)
            {
                List<(int, int)> newPositions = [..positions];
                var canExpand = true;

                foreach (var (x, y) in positions)
                {
                    var nx = x + dx;
                    var ny = y + dy;

                    if (positions.Contains((nx, ny))) continue;
                    if (!IsValidPosition(nx, ny, priority))
                    {
                        canExpand = false;
                        break;
                    }

                    newPositions.Add((nx, ny));
                }

                if (!canExpand) continue;
                {
                    foreach (var (nx, ny) in newPositions)
                        cells[nx][ny] = new Cell
                            { PanelId = cells[positions[0].Item1][positions[0].Item2]?.PanelId, Priority = priority };
                    Fill(newPositions, priority);
                    return;
                }
            }

            return;

            bool IsValidPosition(int x, int y, int currentPriority)
            {
                return x >= 0 && y >= 0 && x < RowRatios.Count && y < ColumnRatios.Count &&
                       !reservedPanels.Contains((x, y)) &&
                       (cells[x][y] is null || cells[x][y]?.Priority <= currentPriority);
            }
        }

        (int x, int y, int width, int height) GetPanelRect(string id)
        {
            List<(int x, int y)> cellsForPanel = [];
            for (var i = 0; i < cells.Count; i++)
            for (var j = 0; j < cells[i].Count; j++)
                if (cells[i][j]?.PanelId == id)
                    cellsForPanel.Add((i, j));

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

    private static List<List<Cell?>> InitializeCells(int rows, int cols)
    {
        var grid = new List<List<Cell?>>();
        for (var i = 0; i < rows; i++) grid.Add([..new Cell[cols]]);
        return grid;
    }

    private double NormalizeRatios(List<double> ratios)
    {
        double sum = 0;
        foreach (var ratio in ratios)
            sum += ratio;

        for (var i = 0; i < ratios.Count; i++)
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
        for (var i = start; i < start + count; i++)
            sum += sizes[i];
        return sum;
    }

    private class Cell
    {
        public string? PanelId;
        public int Priority = -1;
    }
}