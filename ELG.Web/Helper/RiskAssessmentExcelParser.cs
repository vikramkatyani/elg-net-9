using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using System.Linq;
using CsvHelper;
using System.Globalization;

namespace ELG.Web.Helper
{
    public class RiskAssessmentExcelParser
    {
        public class RAOption
        {
            public string Text { get; set; }
            public bool Issue { get; set; }
            public int Order { get; set; }
        }

        public class RAQuestion
        {
            public string Question { get; set; }
            public string Instructions { get; set; }
            public List<RAOption> Options { get; set; } = new List<RAOption>();
        }

        public class RASection
        {
            public string Name { get; set; }
            public List<RAQuestion> Questions { get; set; } = new List<RAQuestion>();
        }

        public class RAParsed
        {
            public List<RASection> Sections { get; set; } = new List<RASection>();
        }

        public static RAParsed Parse(Stream fileStream)
        {
            var parsed = new RAParsed();

            try
            {
                // Try to read as Excel first
                fileStream.Position = 0;
                try
                {
                    ParseExcel(fileStream, parsed);
                    return parsed;
                }
                catch
                {
                    // If Excel parsing fails, try CSV
                    fileStream.Position = 0;
                    ParseCsv(fileStream, parsed);
                    return parsed;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse file: {ex.Message}", ex);
            }
        }

        private static void ParseExcel(Stream fileStream, RAParsed parsed)
        {
            using (var wb = new XLWorkbook(fileStream))
            {
                var ws = wb.Worksheets.Worksheet(1);
                var firstRowUsed = ws.FirstRowUsed();
                if (firstRowUsed == null) return;

                var headerRow = firstRowUsed.RowUsed();
                var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                int lastCell = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
                
                for (int c = 1; c <= lastCell; c++)
                {
                    var name = headerRow.Cell(c).GetString().Trim();
                    if (!string.IsNullOrEmpty(name) && !colMap.ContainsKey(name))
                        colMap[name] = c;
                }

                ValidateRequiredColumns(colMap);

                string Get(string colName, IXLRow row)
                {
                    if (!colMap.TryGetValue(colName, out int idx)) return string.Empty;
                    return row.Cell(idx).GetString().Trim();
                }

                ProcessRows(parsed, ws.RowsUsed().Skip(1), Get);
            }
        }

        private static void ParseCsv(Stream fileStream, RAParsed parsed)
        {
            using (var reader = new StreamReader(fileStream))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                var headers = csv.HeaderRecord;
                var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < headers.Length; i++)
                {
                    if (!string.IsNullOrEmpty(headers[i]) && !colMap.ContainsKey(headers[i]))
                        colMap[headers[i]] = i;
                }

                ValidateRequiredColumns(colMap);

                while (csv.Read())
                {
                    ProcessCsvRow(parsed, csv, colMap);
                }
            }
        }

        private static void ValidateRequiredColumns(Dictionary<string, int> colMap)
        {
            var requiredCols = new[] { "Section", "Question", "Instructions" };
            var missingCols = requiredCols.Where(col => !colMap.ContainsKey(col)).ToList();
            if (missingCols.Any())
            {
                throw new InvalidOperationException($"Missing required columns: {string.Join(", ", missingCols)}");
            }
        }

        private static void ProcessRows(RAParsed parsed, IEnumerable<IXLRow> rows, Func<string, IXLRow, string> getValue)
        {
            var sectionDict = new Dictionary<string, RASection>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                string sectionName = getValue("Section", row);
                string questionText = getValue("Question", row);
                string instructions = getValue("Instructions", row);

                if (string.IsNullOrWhiteSpace(questionText))
                    continue;

                if (string.IsNullOrWhiteSpace(sectionName))
                    sectionName = "General";

                if (!sectionDict.TryGetValue(sectionName, out var section))
                {
                    section = new RASection { Name = sectionName };
                    sectionDict[sectionName] = section;
                    parsed.Sections.Add(section);
                }

                var q = new RAQuestion
                {
                    Question = questionText,
                    Instructions = instructions
                };

                var issueCol = getValue("Issue", row);
                var issueSet = ParseIssueIndices(issueCol);

                int orderSeed = 10;
                for (int i = 1; i <= 6; i++)
                {
                    var optText = getValue($"Option{i}", row);
                    if (string.IsNullOrWhiteSpace(optText)) continue;
                    q.Options.Add(new RAOption
                    {
                        Text = optText,
                        Issue = issueSet.Contains(i),
                        Order = orderSeed
                    });
                    orderSeed += 10;
                }

                section.Questions.Add(q);
            }
        }

        private static void ProcessCsvRow(RAParsed parsed, CsvReader csv, Dictionary<string, int> colMap)
        {
            var sectionDict = parsed.Sections.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

            string GetCsvValue(string colName)
            {
                if (colMap.TryGetValue(colName, out int idx))
                {
                    return csv.GetField(idx)?.Trim() ?? string.Empty;
                }
                return string.Empty;
            }

            string sectionName = GetCsvValue("Section");
            string questionText = GetCsvValue("Question");
            string instructions = GetCsvValue("Instructions");

            if (string.IsNullOrWhiteSpace(questionText))
                return;

            if (string.IsNullOrWhiteSpace(sectionName))
                sectionName = "General";

            if (!sectionDict.TryGetValue(sectionName, out var section))
            {
                section = new RASection { Name = sectionName };
                sectionDict[sectionName] = section;
                parsed.Sections.Add(section);
            }

            var q = new RAQuestion
            {
                Question = questionText,
                Instructions = instructions
            };

            var issueCol = GetCsvValue("Issue");
            var issueSet = ParseIssueIndices(issueCol);

            int orderSeed = 10;
            for (int i = 1; i <= 6; i++)
            {
                var optText = GetCsvValue($"Option{i}");
                if (string.IsNullOrWhiteSpace(optText)) continue;
                q.Options.Add(new RAOption
                {
                    Text = optText,
                    Issue = issueSet.Contains(i),
                    Order = orderSeed
                });
                orderSeed += 10;
            }

            section.Questions.Add(q);
        }

        private static HashSet<int> ParseIssueIndices(string issueCol)
        {
            var issueSet = new HashSet<int>();
            if (!string.IsNullOrWhiteSpace(issueCol))
            {
                foreach (var part in issueCol.Split(','))
                {
                    if (int.TryParse(part.Trim(), out int idx) && idx > 0 && idx <= 6)
                    {
                        issueSet.Add(idx);
                    }
                }
            }
            return issueSet;
        }
    }
}
