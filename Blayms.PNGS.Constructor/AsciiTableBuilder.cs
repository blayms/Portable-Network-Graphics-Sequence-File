using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blayms.PNGS.Constructor
{
    public class AsciiTableBuilder
    {
        public TableAlignment Alignment { get; set; } = TableAlignment.Left;
        public string HorizontalBorder { get; set; } = "-";
        public string VerticalBorder { get; set; } = "|";
        public string CornerBorder { get; set; } = "+";
        public string RowSeparator { get; set; } = "";
        public int CellPadding { get; set; } = 1;
        public bool ShowHeaders { get; set; } = true;
        public int MaxCellWidth { get; set; } = 30;

        private string[] _headers = Array.Empty<string>();
        private readonly List<string[]> _rows = new List<string[]>();

        /// <summary>Sets table headers</summary>
        public void SetHeaders(params string[] headers)
        {
            _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        }

        /// <summary>Adds a row to the table</summary>
        public void AddRow(params string[] row)
        {
            _rows.Add(row ?? throw new ArgumentNullException(nameof(row)));
        }

        /// <summary>Adds multiple rows</summary>
        public void AddRows(IEnumerable<string[]> rows)
        {
            foreach (var row in rows)
            {
                AddRow(row);
            }
        }

        /// <summary>Generates the ASCII table string</summary>
        public override string ToString()
        {
            if (_headers.Length == 0 && !_rows.Any())
                return string.Empty;

            var columnWidths = CalculateColumnWidths();
            var builder = new StringBuilder();
            string indentStr = ConsoleEx.BuildIndent();

            // Top border
            builder.Append(indentStr).AppendLine(BuildBorder(columnWidths));

            // Headers
            if (ShowHeaders && _headers.Length > 0)
            {
                var headerLines = BuildMultiLineRow(_headers, columnWidths);
                foreach (var line in headerLines)
                {
                    builder.Append(indentStr).AppendLine(line);
                }
                builder.Append(indentStr).AppendLine(BuildBorder(columnWidths));
            }

            // Rows with optional separators
            for (int i = 0; i < _rows.Count; i++)
            {
                var rowLines = BuildMultiLineRow(_rows[i], columnWidths);
                foreach (var line in rowLines)
                {
                    builder.Append(indentStr).AppendLine(line);
                }

                // Add separator after each row except last
                if (!string.IsNullOrEmpty(RowSeparator) && i < _rows.Count - 1)
                {
                    builder.Append(indentStr).AppendLine(BuildSeparator(columnWidths));
                }
            }

            // Bottom border
            builder.Append(indentStr).AppendLine(BuildBorder(columnWidths));

            return builder.ToString();
        }

        private string[] BuildMultiLineRow(string[] cells, int[] columnWidths)
        {
            // Split each cell into multiple lines
            var cellLines = cells.Select((cell, i) =>
                SplitIntoLines(cell, i < columnWidths.Length ? columnWidths[i] : MaxCellWidth)
            ).ToArray();

            // Determine how many lines we need for each cell
            int maxLines = cellLines.Max(cl => cl.Length);

            var result = new List<string>();
            for (int lineIndex = 0; lineIndex < maxLines; lineIndex++)
            {
                var lineCells = new string[cells.Length];
                for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                {
                    var lines = cellLines[cellIndex];
                    string cellContent = lineIndex < lines.Length ? lines[lineIndex] : "";
                    int cellWidth = cellIndex < columnWidths.Length ? columnWidths[cellIndex] : 0;

                    lineCells[cellIndex] = AlignContent(cellContent, cellWidth);
                }
                result.Add(BuildSingleLineRow(lineCells, columnWidths));
            }

            return result.ToArray();
        }

        private string[] SplitIntoLines(string content, int maxWidth)
        {
            if (maxWidth <= 0 || content.Length <= maxWidth)
                return new[] { content };

            var lines = new List<string>();
            for (int i = 0; i < content.Length; i += maxWidth)
            {
                int length = Math.Min(maxWidth, content.Length - i);
                lines.Add(content.Substring(i, length));
            }

            return lines.ToArray();
        }

        private string AlignContent(string content, int width)
        {
            switch (Alignment)
            {
                case TableAlignment.Left:
                    return content.PadRight(width);
                case TableAlignment.Right:
                    return content.PadLeft(width);
                case TableAlignment.Middle:
                    int totalPadding = width - content.Length;
                    int leftPadding = totalPadding / 2;
                    int rightPadding = totalPadding - leftPadding;
                    return new string(' ', leftPadding) + content + new string(' ', rightPadding);
                default:
                    return content;
            }
        }

        private string BuildSingleLineRow(string[] cells, int[] columnWidths)
        {
            var cellSpacing = new string(' ', CellPadding);
            return VerticalBorder + cellSpacing +
                   string.Join(cellSpacing + VerticalBorder + cellSpacing, cells) +
                   cellSpacing + VerticalBorder;
        }

        private string BuildSeparator(int[] columnWidths)
        {
            var parts = columnWidths.Select(w =>
                new string(RowSeparator[0], w + (CellPadding * 2))
            );
            return CornerBorder + string.Join(CornerBorder, parts) + CornerBorder;
        }

        private int[] CalculateColumnWidths()
        {
            var allRows = _rows.ToList();
            if (ShowHeaders && _headers.Length > 0)
            {
                allRows.Insert(0, _headers);
            }

            if (!allRows.Any())
                return Array.Empty<int>();

            int columnCount = allRows.Max(r => r.Length);
            var widths = new int[columnCount];

            foreach (var row in allRows)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    // For width calculation, we consider either the full length or MaxCellWidth,
                    // whichever is smaller, since we'll wrap longer text
                    int cellWidth = Math.Min(row[i].Length, MaxCellWidth);
                    widths[i] = Math.Max(widths[i], cellWidth);
                }
            }

            return widths;
        }

        private string BuildBorder(int[] columnWidths)
        {
            var parts = columnWidths.Select(w =>
                new string(HorizontalBorder[0], w + (CellPadding * 2))
            );
            return CornerBorder + string.Join(CornerBorder, parts) + CornerBorder;
        }
    }

    public enum TableAlignment { Left, Right, Middle }
}