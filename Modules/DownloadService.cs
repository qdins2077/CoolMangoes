using System;
using System.IO;
using System.Collections.Generic;
using ClosedXML.Excel;  // Using ClosedXML for Excel creation
using CoolMangoes.Models;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Text;
//ExcelPackage.LicenseContext = LicenseContext.NonCommercial;



namespace CoolMangoes.Modules
{
    public class DownloadService
    {
        private readonly string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        private readonly WorkbookSheetService _workbookSheetService;

        public DownloadService(WorkbookSheetService workbookSheetService)
        {
            _workbookSheetService = workbookSheetService;
        }

        public void DownloadAssetDataTemplate()
        {
            string fileName = "AssetDataTemplate.xlsx";
            string filePath = Path.Combine(downloadsFolder, fileName);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Asset Data");
                
                // Define columns in correct order (left to right)
                var columns = new[]
                {
                    "Location1",
                    "Location2",
                    "Location3",
                    "Location4",
                    "Asset_ID",
                    "Parent_ID",
                    "AssetDescription",
                    "AssetHierarchy",
                    "HierarchyL1",
                    "HierarchyL2",
                    "HierarchyL3",
                    "HierarchyL4",
                    "HierarchyCode",
                    "Manufacturer",
                    "ModelNumber",
                    "ManufSerialNo",
                    "AcqDate",
                    "ConditionRating",
                    "CurrentUsage",
                    "OperatingEnvironment",
                    "PurchaseCost",
                    "ObservationDate",
                    "MaintenanceStrategyCode",
                    "MaintenanceType",
                    "Statutory",
                    "PlannedStartDate",
                    "PlannedEndDate",
                    "UnderWarranty",
                    "WarrantyStartDate",
                    "WarrantyEndDate",
                    "Injury",
                    "Environmental",
                    "BusinessContinuity",
                    "Reputation",
                    "LossImpactOnTheCompany",
                    "HighestCriticality"
                };

                // Add headers
                for (int i = 0; i < columns.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = columns[i];
                }

                // Format header row
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                
                // Auto-fit columns
                worksheet.Columns().AdjustToContents();
                
                workbook.SaveAs(filePath);
            }
        }

        public void DownloadClassDataTemplate()
        {
            string fileName = "ClassDataTemplate.csv";
            string filePath = Path.Combine(downloadsFolder, fileName);
            string csvHeadings = "AssetHierarchy,HierarchyCode,AssetType,MaintenanceType,Statutory," +
                "EstimatedLife,RefurbishmentFrequency,RefurbishmentCostAsProportionOfReplacementCost," +
                "MinCost,MaxCost,AvgReplacementCost," +
                "Injury,Environmental,BusinessContinuity,Reputation,LossImpactOnTheCompany,HighestCriticality";

            WriteCsvTemplate(filePath, csvHeadings);
        }

        public void DownloadMaintenanceStrategies(string filePath)
        {
            string csvHeadings = "StrategyCode,StrategyDescription,Cost/hour,ResourceType,ResourceName";
            WriteCsvTemplate(filePath, csvHeadings);
        }

        public void DownloadMaintenanceProcedures(string filePath)
        {
            string csvHeadings = "StrategyCode,StrategyDescription,HierarchyL1,HierarchyL2,HierarchyL3,HierarchyL4,ProcedureCode,ProcedureDescription,Duration,Frequency,FrequencyType,MaintenanceStatus,Statutory";
            WriteCsvTemplate(filePath, csvHeadings);
        }

         public void DownloadCapitalProjectsTemplate()
        {
            string fileName = "CapitalProjectsTemplate.csv";
            string filePath = Path.Combine(downloadsFolder, fileName);
            string csvHeadings = "Location1,Location2,Location3,Location4,Asset_ID,ProjectCategory,ProjectTitle,ProjectCost,ProjectStartYear,ProjectEndYear";

            WriteCsvTemplate(filePath, csvHeadings);
        }


        private void WriteCsvTemplate(string filePath, string csvHeadings)
        {
            try
            {
                File.WriteAllText(filePath, csvHeadings);
                Console.WriteLine($"Template has been successfully saved to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
            }
        }

     

        public void DownloadLCCCostModel(
            IEnumerable<Expenditure> expenditureData, 
            List<Asset> assetData,
            List<ClassData> classData,
            List<MaintenanceStrategy> maintenanceStrategies,
            List<MaintenanceProcedure> maintenanceProcedures,
            DateTime startDate,
            DateTime endDate,
            string filePath,
            IProgress<int> progress = null)
        {
            if (expenditureData == null)
            {
                throw new ArgumentNullException(nameof(expenditureData));
            }

            using (var workbook = new XLWorkbook())
            {
                _workbookSheetService.AddHomeSheet(workbook, startDate, endDate);
                _workbookSheetService.AddEquipmentDataSheet(workbook, assetData);
                _workbookSheetService.AddClassDataSheet(workbook, classData);
                _workbookSheetService.AddMaintenanceStrategiesSheet(workbook, maintenanceStrategies);
                _workbookSheetService.AddMaintenanceProceduresSheet(workbook, maintenanceProcedures);

                var worksheet = workbook.Worksheets.Add("ExpenditurePlan");
                
                // Set global styles
                worksheet.Style.Font.FontName = "Helvetica";
                worksheet.Style.Font.FontSize = 11;

                // Add headers
                var headers = new string[]
                {
                    "Location1", "Location2", "Location3", "Location4", "Asset_ID", "AssetDescription",
                    "HierarchyL1", "HierarchyL2", "HierarchyL3", "HierarchyL4", "AssetHierarchy", "HierarchyCode",
                    "ExpenditureValue", "ExpenditureDate", "ExpenditureYear", "ExpenditureType", 
                    "ExpenditureDescription", "Comment"
                };

                // Add header row
                int startingRow = 9;
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(startingRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#00365E");
                    cell.Style.Font.FontColor = XLColor.White;
                }

                // Add data rows
                int row = startingRow + 1;
                foreach (var expenditure in expenditureData)
                {
                    worksheet.Cell(row, 1).Value = expenditure.Location1 ?? "";
                    worksheet.Cell(row, 2).Value = expenditure.Location2 ?? "";
                    worksheet.Cell(row, 3).Value = expenditure.Location3 ?? "";
                    worksheet.Cell(row, 4).Value = expenditure.Location4 ?? "";
                    worksheet.Cell(row, 5).Value = expenditure.Asset_ID ?? "";
                    worksheet.Cell(row, 6).Value = expenditure.AssetDescription ?? "";
                    worksheet.Cell(row, 7).Value = expenditure.HierarchyL1 ?? "";
                    worksheet.Cell(row, 8).Value = expenditure.HierarchyL2 ?? "";
                    worksheet.Cell(row, 9).Value = expenditure.HierarchyL3 ?? "";
                    worksheet.Cell(row, 10).Value = expenditure.HierarchyL4 ?? "";
                    worksheet.Cell(row, 11).Value = expenditure.AssetHierarchy ?? "";
                    worksheet.Cell(row, 12).Value = expenditure.HierarchyCode ?? "";
                    worksheet.Cell(row, 13).Value = expenditure.ExpenditureValue;
                    worksheet.Cell(row, 14).Value = expenditure.ExpenditureDate;
                    worksheet.Cell(row, 15).Value = expenditure.ExpenditureYear;
                    worksheet.Cell(row, 16).Value = expenditure.ExpenditureType ?? "";
                    worksheet.Cell(row, 17).Value = expenditure.ExpenditureDescription ?? "";
                    worksheet.Cell(row, 18).Value = expenditure.Comment ?? "";

                    row++;
                }

                workbook.SaveAs(filePath);
                progress?.Report(100);
            }
        }
    
        public void DownloadPreventativeMaintenance(IEnumerable<Expenditure> expenditureData, string filePath, IProgress<int> progress = null)
        {
            if (expenditureData == null)
            {
                throw new ArgumentNullException(nameof(expenditureData), "Expenditure data cannot be null.");
            }

            // Update filter to match the ExpenditureType exactly
            var pmData = expenditureData.Where(e => 
                e.ExpenditureType?.Equals("Preventative Maintenance", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            if (!pmData.Any())
            {
                Console.WriteLine("No PM data found after filtering.");
                return;
            }
            
            const int maxRowsPerFile = 1000000; // Excel limit is roughly 1,048,576 rows
            
            if (pmData.Count <= maxRowsPerFile)
            {
                // Single file download
                DownloadPMToExcel(pmData, filePath, progress);
            }
            else
            {
                // Split into multiple files
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                var fileExtension = Path.GetExtension(filePath);
                var directory = Path.GetDirectoryName(filePath);

                for (int i = 0; i < Math.Ceiling((double)pmData.Count / maxRowsPerFile); i++)
                {
                    var chunk = pmData.Skip(i * maxRowsPerFile).Take(maxRowsPerFile);
                    var newFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_Part{i + 1}{fileExtension}");
                    DownloadPMToExcel(chunk, newFilePath, progress);
                }
            }
        }

        private void DownloadPMToExcel(IEnumerable<Expenditure> pmData, string filePath, IProgress<int> progress = null)
        {
            // Log the start of the process and count of records
            Console.WriteLine($"Starting to write PM records to Excel. Record count: {pmData.Count()}");

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    // Create a new worksheet named "PreventativeMaintenance"
                    var worksheet = workbook.Worksheets.Add("PreventativeMaintenance");

                    // Set global styles
                    worksheet.Style.Font.FontName = "Helvetica";
                    worksheet.Style.Font.FontSize = 11;

                    // Define the column headers
                    var headers = new string[]
                    {
                        "Location1", "Location2", "Location3", "Location4", 
                        "Asset_ID", "AssetDescription",
                        "HierarchyL1", "HierarchyL2", "HierarchyL3", "HierarchyL4", 
                        "AssetHierarchy", "HierarchyCode",
                        "ExpenditureValue", "ExpenditureDate", "ExpenditureYear", 
                        "ExpenditureDescription", "Comment"
                    };

                    // Write headers to the first row
                    for (int colIndex = 0; colIndex < headers.Length; colIndex++)
                    {
                        var headerCell = worksheet.Cell(1, colIndex + 1);
                        headerCell.Value = headers[colIndex];
                        headerCell.Style.Font.Bold = true;
                        headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#00365E");
                        headerCell.Style.Font.FontColor = XLColor.White;
                    }

                    // Initialize row counter (start from row 2 as row 1 has headers)
                    int currentRow = 2;
                    int totalRecords = pmData.Count();
                    int processedRecords = 0;

                    // Write each PM record to the worksheet
                    foreach (Expenditure pmRecord in pmData)
                    {
                        Console.WriteLine($"Processing record {processedRecords + 1} of {totalRecords}: Asset {pmRecord.Asset_ID}");

                        // Write data to each cell in the current row
                        worksheet.Cell(currentRow, 1).Value = pmRecord.Location1 ?? "";
                        worksheet.Cell(currentRow, 2).Value = pmRecord.Location2 ?? "";
                        worksheet.Cell(currentRow, 3).Value = pmRecord.Location3 ?? "";
                        worksheet.Cell(currentRow, 4).Value = pmRecord.Location4 ?? "";
                        worksheet.Cell(currentRow, 5).Value = pmRecord.Asset_ID ?? "";
                        worksheet.Cell(currentRow, 6).Value = pmRecord.AssetDescription ?? "";
                        worksheet.Cell(currentRow, 7).Value = pmRecord.HierarchyL1 ?? "";
                        worksheet.Cell(currentRow, 8).Value = pmRecord.HierarchyL2 ?? "";
                        worksheet.Cell(currentRow, 9).Value = pmRecord.HierarchyL3 ?? "";
                        worksheet.Cell(currentRow, 10).Value = pmRecord.HierarchyL4 ?? "";
                        worksheet.Cell(currentRow, 11).Value = pmRecord.AssetHierarchy ?? "";
                        worksheet.Cell(currentRow, 12).Value = pmRecord.HierarchyCode ?? "";
                        worksheet.Cell(currentRow, 13).Value = pmRecord.ExpenditureValue;
                        worksheet.Cell(currentRow, 14).Value = pmRecord.ExpenditureDate.ToShortDateString();
                        worksheet.Cell(currentRow, 15).Value = pmRecord.ExpenditureDate.Year;
                        worksheet.Cell(currentRow, 16).Value = pmRecord.ExpenditureDescription ?? "";
                        worksheet.Cell(currentRow, 17).Value = pmRecord.Comment ?? "";

                        // Move to next row
                        currentRow++;
                        processedRecords++;

                        // Update progress
                        if (progress != null)
                        {
                            int percentComplete = (processedRecords * 100) / totalRecords;
                            progress.Report(percentComplete);
                        }
                    }

                    // Adjust column widths to fit content
                    worksheet.Columns().AdjustToContents();

                    Console.WriteLine("Finished processing records. Saving workbook...");

                    // Save the workbook
                    workbook.SaveAs(filePath);
                    Console.WriteLine($"Workbook saved successfully to: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DownloadPMToExcel: {ex.Message}");
                throw; // Re-throw the exception to be handled by the calling method
            }
        }

        public void DownloadHierarchyTemplate(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Hierarchy");
                
                // Set columns in correct order (left to right)
                worksheet.Cell(1, 1).Value = "Tree";  // For hierarchy visualization
                worksheet.Cell(1, 2).Value = "Description";
                worksheet.Cell(1, 3).Value = "ID";
                
                // Format header row
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                
                // Set column widths and alignment
                worksheet.Column(1).Width = 20;  // For tree structure
                worksheet.Column(2).Width = 50;  // For description
                worksheet.Column(3).Width = 15;  // For ID
                
                // Set text alignment
                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                
                // Set font to Consolas for better tree visualization
                worksheet.Style.Font.FontName = "Consolas";
                
                // Ensure proper column order by setting view properties
                worksheet.SheetView.FreezeColumns(1);  // This helps maintain column order
                
                workbook.SaveAs(filePath);
            }
        }

        private List<HierarchyItem> BuildHierarchyTree(List<HierarchyItem> items)
        {
            var lookup = items.ToLookup(item => item.Parent_ID);
            var root = new List<HierarchyItem>();

            foreach (var item in items.Where(i => string.IsNullOrEmpty(i.Parent_ID)))
            {
                item.Level = 0;
                root.Add(item);
                AddChildren(item, lookup);
            }

            return root;
        }

        private void AddChildren(HierarchyItem parent, ILookup<string, HierarchyItem> lookup)
        {
            foreach (var child in lookup[parent.ID])
            {
                child.Level = parent.Level + 1;
                parent.Children.Add(child);
                AddChildren(child, lookup);
            }
        }

        public List<HierarchyItem> ValidateHierarchyTemplate(string filePath, List<string> assetIds)
        {
            try
            {
                var hierarchyData = new List<HierarchyItem>();
                var ids = new HashSet<string>();

                // First pass: collect all IDs
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;

                    // Skip header
                    parser.ReadLine();

                    while (!parser.EndOfData)
                    {
                        string[] values = parser.ReadFields();
                        if (values?.Length >= 3)
                        {
                            var id = values[0].Trim();
                            
                            // Check for duplicate IDs
                            if (ids.Contains(id))
                            {
                                throw new Exception($"Duplicate ID found: {id}");
                            }

                            ids.Add(id);
                        }
                    }
                }

                // Second pass: create hierarchy items and validate parent IDs
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;

                    // Skip header
                    parser.ReadLine();

                    while (!parser.EndOfData)
                    {
                        string[] values = parser.ReadFields();
                        if (values?.Length >= 3)
                        {
                            var id = values[0].Trim();
                            var description = values[1].Trim();
                            var parentId = values[2].Trim();

                            // Validate that parent ID exists in our ID list
                            if (!string.IsNullOrWhiteSpace(parentId) && !ids.Contains(parentId))
                            {
                                throw new Exception($"Invalid Parent_ID found: '{parentId}' for ID: '{id}'. Parent_ID must exist as an ID in another row.");
                            }

                            var newItem = new HierarchyItem
                            {
                                ID = id,
                                Description = description,
                                Parent_ID = string.IsNullOrWhiteSpace(parentId) ? null : parentId,
                                Children = new List<HierarchyItem>()  // Initialize Children list
                            };

                            if (HasCircularReference(id, parentId, hierarchyData))
                            {
                                throw new Exception($"Circular reference detected for ID: {id} with Parent_ID: {parentId}");
                            }

                            hierarchyData.Add(newItem);
                        }
                    }
                }

                // Verify all asset IDs are present
                var missingAssets = assetIds.Where(id => !ids.Contains(id)).ToList();
                if (missingAssets.Any())
                {
                    throw new Exception($"Missing asset IDs: {string.Join(", ", missingAssets)}");
                }

                // Build the hierarchy tree
                return BuildHierarchyTree(hierarchyData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating hierarchy template: {ex.Message}", ex);
            }
        }

        private bool HasCircularReference(string id, string parentId, List<HierarchyItem> items)
        {
            if (string.IsNullOrEmpty(parentId)) return false;
            if (id == parentId) return true;

            var parent = items.FirstOrDefault(i => i.ID == parentId);
            return parent != null && HasCircularReference(id, parent.Parent_ID, items);
        }

        public string GenerateHierarchyPreview(List<HierarchyItem> items)
        {
            var preview = new StringBuilder();
            preview.AppendLine($"{"Description",-80}ID");  // Increased spacing
            preview.AppendLine(new string('─', 100));  // Longer separator line

            foreach (var item in items.Where(i => string.IsNullOrEmpty(i.Parent_ID)))
            {
                GenerateHierarchyLine(item, "", true, preview);
            }
            return preview.ToString();
        }

        private List<List<HierarchyItem>> GroupHierarchyItems(List<HierarchyItem> items)
        {
            var groups = new List<List<HierarchyItem>>();
            var currentGroup = new List<HierarchyItem>();
            string currentPrefix = "";

            foreach (var item in items.OrderBy(i => i.ID))
            {
                // Get the prefix (e.g., "0201-080" from "0201-080-020")
                string prefix = GetIDPrefix(item.ID);
                
                if (prefix != currentPrefix && currentGroup.Any())
                {
                    groups.Add(currentGroup);
                    currentGroup = new List<HierarchyItem>();
                }
                
                currentGroup.Add(item);
                currentPrefix = prefix;
            }
            
            if (currentGroup.Any())
            {
                groups.Add(currentGroup);
            }
            
            return groups;
        }

        private string GetIDPrefix(string id)
        {
            var parts = id.Split('-');
            return parts.Length > 1 ? string.Join("-", parts.Take(2)) : parts[0];
        }

        private void GenerateGroupLines(List<HierarchyItem> group, StringBuilder preview)
        {
            // Add group separator
            preview.AppendLine();
            preview.AppendLine($"{"",3}{"─".PadRight(60, '─')}");

            foreach (var item in group)
            {
                GenerateHierarchyLine(item, "", true, preview);
            }
        }

        private void GenerateHierarchyLine(HierarchyItem item, string indent, bool isLast, StringBuilder preview)
        {
            string treeChar = isLast ? "└" : "├";
            string lineIndent = indent + treeChar + "───";  // Added an extra dash
            
            // Add extra spacing before and after the line characters
            preview.AppendLine($"{lineIndent}  {item.Description,-80}{item.ID}");  // Added two spaces after the line
            
            if (item.Children.Any())
            {
                for (int i = 0; i < item.Children.Count; i++)
                {
                    string newIndent = indent + (isLast ? "    " : "│   ");  // Added extra spaces
                    GenerateHierarchyLine(item.Children[i], newIndent, i == item.Children.Count - 1, preview);
                }
            }
        }

        private string GetLineStyle(string id)
        {
            // Return different line styles based on ID depth
            var parts = id.Split('-');
            switch (parts.Length)
            {
                case 1: return ""; // Top level
                case 2: return "│"; // Second level
                case 3: return "├──"; // Third level
                default: return "└──"; // Deepest level
            }
        }

        private void AddItemToPreview(HierarchyItem item, System.Text.StringBuilder preview, int level)
        {
            // Add indentation based on level
            string indent = new string(' ', level * 4);
            preview.AppendLine($"{indent}{item.ID} - {item.Description}");

            // Recursively add children
            foreach (var child in item.Children)
            {
                AddItemToPreview(child, preview, level + 1);
            }
        }

        private void PopulateHierarchyData(IXLWorksheet worksheet, List<HierarchyItem> hierarchyItems, 
            List<Asset> assets, List<ClassData> classData, List<Expenditure> expenditures,
            DateTime projectStartDate, DateTime projectEndDate, int startRow = 4)
        {
            int currentRow = startRow;
            foreach (var item in hierarchyItems)
            {
                currentRow = PopulateHierarchyItem(worksheet, item, currentRow, 0, assets, classData, 
                    expenditures, projectStartDate, projectEndDate);
            }
        }

        private int PopulateHierarchyItem(IXLWorksheet worksheet, HierarchyItem item, int row, int level,
            List<Asset> assets, List<ClassData> classData, List<Expenditure> expenditures,
            DateTime projectStartDate, DateTime projectEndDate)
        {
            int projectYears = projectEndDate.Year - projectStartDate.Year + 1;

            // Hierarchy columns (A-C)
            string indent = new string(' ', level * 2);
            worksheet.Cell(row, 1).Value = indent + item.ID;
            worksheet.Cell(row, 2).Value = item.Description;
            worksheet.Cell(row, 3).Value = item.ID;

            // Look up asset data
            var asset = assets.FirstOrDefault(a => a.Asset_ID == item.ID);
            if (asset != null)
            {
                // Look up class data using HierarchyCode
                var classInfo = classData.FirstOrDefault(c => c.HierarchyCode == asset.HierarchyCode);
                if (classInfo != null)
                {
                    // Populate criticality data (columns D-I)
                    worksheet.Cell(row, 4).Value = classInfo.Injury;
                    worksheet.Cell(row, 5).Value = classInfo.Environmental;
                    worksheet.Cell(row, 6).Value = classInfo.BusinessContinuity;
                    worksheet.Cell(row, 7).Value = classInfo.Reputation;
                    worksheet.Cell(row, 8).Value = classInfo.LossImpactOnTheCompany;
                    worksheet.Cell(row, 9).Value = classInfo.HighestCriticality;

                    // Group all expenditures for this asset by year and type
                    var assetExpenditures = expenditures
                        .Where(e => e.Asset_ID == asset.Asset_ID)
                        .GroupBy(e => new { e.ExpenditureYear, e.ExpenditureType })
                        .ToDictionary(
                            g => (g.Key.ExpenditureYear, g.Key.ExpenditureType),
                            g => g.Sum(e => e.ExpenditureValue)
                        );
                    
                    // Section 1: RUL values
                    int startCol = 10;
                    PopulateYearlyValues(worksheet, row, startCol, projectStartDate, projectEndDate, 
                        year => assetExpenditures.TryGetValue((year, "Replacement"), out var _) ? 100.0 :
                            assetExpenditures.TryGetValue((year, "Refurbishment"), out var _) ? 80.0 :
                            assetExpenditures.TryGetValue((year, "RUL"), out var rulValue) ? rulValue : 0.0);

                    // Section 2: PM values
                    startCol = 10 + projectYears;
                    PopulateYearlyValues(worksheet, row, startCol, projectStartDate, projectEndDate,
                        year => assetExpenditures.TryGetValue((year, "Preventative Maintenance"), out var pmValue) ? pmValue : 0.0);

                    // Section 3: CM values
                    startCol = 10 + (2 * projectYears);
                    PopulateYearlyValues(worksheet, row, startCol, projectStartDate, projectEndDate,
                        year => assetExpenditures.TryGetValue((year, "Corrective Maintenance"), out var cmValue) ? cmValue : 0.0);

                    // Section 4: LoF values
                    startCol = 10 + (3 * projectYears);
                    PopulateYearlyValues(worksheet, row, startCol, projectStartDate, projectEndDate,
                        year => assetExpenditures.TryGetValue((year, "LoF"), out var lofValue) ? lofValue : 0);

                    // Section 5: Replacement values
                    startCol = 10 + (4 * projectYears);
                    PopulateYearlyValues(worksheet, row, startCol, projectStartDate, projectEndDate,
                        year => assetExpenditures.TryGetValue((year, "Replacement"), out var replValue) ? replValue : 0.0);

                    // Section 6: Refurbishment values
                    startCol = 10 + (5 * projectYears);
                    PopulateYearlyValues(worksheet, row, startCol, projectStartDate, projectEndDate,
                        year => assetExpenditures.TryGetValue((year, "Refurbishment"), out var refValue) ? refValue : 0.0);

                    // Section 7: Capital Project values
                    startCol = 10 + (6 * projectYears);
                    PopulateYearlyValues(worksheet, row, startCol, projectStartDate, projectEndDate,
                        year => assetExpenditures.TryGetValue((year, "Capital Project"), out var cpValue) ? cpValue : 0.0);

                    // Section 8: Risk values
                    startCol = 10 + (7 * projectYears);
                    PopulateYearlyValues(worksheet, row, startCol, projectStartDate, projectEndDate, year =>
                    {
                        // Get LoF for this year
                        var lof = assetExpenditures.TryGetValue((year, "LoF"), out var lofValue) ? (int)lofValue : 1;
                        
                        // Get highest criticality
                        var criticality = classInfo.HighestCriticality;
                        
                        // Look up risk value from matrix
                        return GetRiskValue(lof, criticality);
                    });

                    // Apply number formatting to all numeric columns
                    ApplyNumberFormatting(worksheet, row, projectYears);
                }
                
            }

            // For each child, recursively populate and update the row number
            foreach (var child in item.Children)
            {
                row = PopulateHierarchyItem(worksheet, child, row, level + 1,
                    assets, classData, expenditures, projectStartDate, projectEndDate);
            }

            // Return the next available row
            return row + 1;
        }

        private string FormatCellValue(object value)
        {
            if (value == null) return "";
            
            // Handle numeric values
            if (value is double || value is int || value is decimal)
            {
                var numValue = Convert.ToDouble(value);
                if (numValue == 0) return "";
                return numValue.ToString("N0"); // Format with thousand separators, no decimals
            }

            // Handle string values
            var strValue = value.ToString().Trim();
            if (string.IsNullOrWhiteSpace(strValue) || 
                strValue.Equals("NULL", StringComparison.OrdinalIgnoreCase) || 
                strValue.Equals("#N/A", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("NA", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("#VALUE", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("#####", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("#DIV/0!", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("#REF!", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("#NULL!", StringComparison.OrdinalIgnoreCase) ||
                strValue.Equals("#NUM!", StringComparison.OrdinalIgnoreCase) ||
                strValue == "0")
            {
                return "";
            }

            return strValue;
        }

        private void PopulateCell(IXLCell cell, object value)
        {
            cell.Value = FormatCellValue(value);
            
            // If it's a number and not empty, right-align
            if (value is double || value is int || value is decimal)
            {
                var numValue = Convert.ToDouble(value);
                if (numValue != 0)
                {
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
            }
        }
        

        private void PopulateYearlyValues(IXLWorksheet worksheet, int row, int startColumn, 
            DateTime startDate, DateTime endDate, Func<int, double> valueSelector)
        {
            int years = endDate.Year - startDate.Year + 1;
            for (int i = 0; i < years; i++)
            {
                var cell = worksheet.Cell(row, startColumn + i);
                var value = valueSelector(startDate.Year + i);
                PopulateCell(cell, value);
            }
        }

        private void ApplyNumberFormatting(IXLWorksheet worksheet, int row, int projectYears)
        {
            // RUL formatting (percentages)
            var rulRange = worksheet.Range(row, 10, row, 9 + projectYears);
            rulRange.Style.NumberFormat.Format = "0.00%";

            // Financial formatting (PM, CM, Replacement, Refurbishment, Capital Project)
            var financialRange = worksheet.Range(row, 10 + projectYears, row, 9 + (6 * projectYears));
            financialRange.Style.NumberFormat.Format = "#,##0.00";

            // LoF formatting (whole numbers)
            var lofRange = worksheet.Range(row, 10 + (3 * projectYears), row, 9 + (4 * projectYears));
            lofRange.Style.NumberFormat.Format = "0";

            // Risk formatting with updated color scheme
            var riskRange = worksheet.Range(row, 10 + (7 * projectYears), row, 9 + (8 * projectYears));
            riskRange.Style.NumberFormat.Format = "0";
            ApplyRiskFormatting(riskRange);
        }

        // Add new method to handle non-asset Capital Projects
        private int AddNonAssetCapitalProjects(IXLWorksheet worksheet, List<Expenditure> expenditures, 
            DateTime startDate, DateTime endDate, int startRow)
        {
            var nonAssetProjects = expenditures
                .Where(e => string.IsNullOrEmpty(e.Asset_ID) && 
                    e.ExpenditureType?.Equals("Capital Project", StringComparison.OrdinalIgnoreCase) == true)
                .GroupBy(e => e.ExpenditureDescription)
                .ToList();

            int currentRow = startRow;
            int projectYears = endDate.Year - startDate.Year + 1;

            foreach (var project in nonAssetProjects)
            {
                worksheet.Cell(currentRow, 1).Value = "Capital Project";
                worksheet.Cell(currentRow, 2).Value = project.Key; // ProjectTitle
                worksheet.Cell(currentRow, 3).Value = "Capital Project";

                var rowRange = worksheet.Range(currentRow, 1, currentRow, worksheet.LastColumnUsed().ColumnNumber());
                rowRange.Style.Fill.BackgroundColor = XLColor.White;

                int startCol = 10 + (projectYears * 6);
                foreach (var expenditure in project.OrderBy(e => e.ExpenditureYear))
                {
                    int colOffset = expenditure.ExpenditureYear - startDate.Year;
                    worksheet.Cell(currentRow, startCol + colOffset).Value = expenditure.ExpenditureValue;
                }

                currentRow++;
            }

            return currentRow;
        }

        private int GetLoFFromCMValue(double cmValue, double pmValue)
        {
            if (pmValue == 0) return 1;
            double ratio = cmValue / pmValue;
            
            if (ratio <= 0.12) return 1;
            if (ratio <= 0.24) return 2;
            if (ratio <= 0.36) return 4;
            if (ratio <= 0.48) return 16;
            if (ratio <= 0.60) return 32;
            return 64;
        }
        private int GetTotalItemCount(HierarchyItem item)
        {
            int count = 1; // Count this item
            foreach (var child in item.Children)
            {
                count += GetTotalItemCount(child);
            }
            return count;
        }

        private readonly Dictionary<(int Likelihood, int Criticality), int> _riskMatrix = new()
        {
            // Almost Certain (64)
            { (64, 1), 64 },    { (64, 3), 192 },   { (64, 7), 448 },    { (64, 15), 960 },   { (64, 40), 2560 },  { (64, 100), 6400 },
            // Likely (32)
            { (32, 1), 32 },    { (32, 3), 96 },    { (32, 7), 224 },    { (32, 15), 480 },   { (32, 40), 1280 },  { (32, 100), 3200 },
            // Possible (16)
            { (16, 1), 16 },    { (16, 3), 48 },    { (16, 7), 112 },    { (16, 15), 240 },   { (16, 40), 640 },   { (16, 100), 1600 },
            // Unlikely (4)
            { (4, 1), 4 },      { (4, 3), 12 },     { (4, 7), 28 },      { (4, 15), 60 },     { (4, 40), 160 },    { (4, 100), 400 },
            // Highly Unlikely (2)
            { (2, 1), 2 },      { (2, 3), 6 },      { (2, 7), 14 },      { (2, 15), 30 },     { (2, 40), 80 },     { (2, 100), 200 },
            // Very Rare (1)
            { (1, 1), 1 },      { (1, 3), 3 },      { (1, 7), 7 },       { (1, 15), 15 },     { (1, 40), 40 },     { (1, 100), 100 }
        };
        
        private int GetRiskValue(int likelihood, int criticality)
        {
            // Normalize criticality to matrix values
            int normalizedCriticality;
            if (criticality <= 1) normalizedCriticality = 1;
            else if (criticality <= 3) normalizedCriticality = 3;
            else if (criticality <= 7) normalizedCriticality = 7;
            else if (criticality <= 15) normalizedCriticality = 15;
            else if (criticality <= 40) normalizedCriticality = 40;
            else normalizedCriticality = 100;

            // Normalize likelihood to matrix values
            int normalizedLikelihood;
            if (likelihood <= 1) normalizedLikelihood = 1;
            else if (likelihood <= 2) normalizedLikelihood = 2;
            else if (likelihood <= 4) normalizedLikelihood = 4;
            else if (likelihood <= 16) normalizedLikelihood = 16;
            else if (likelihood <= 32) normalizedLikelihood = 32;
            else normalizedLikelihood = 64;

            return _riskMatrix.TryGetValue((normalizedLikelihood, normalizedCriticality), out int value) 
                ? value 
                : 0;
        }

        // When you need to handle nullable integers, use this overload:
        private int GetRiskValue(int? likelihood, int? criticality)
        {
            return GetRiskValue(
                likelihood.GetValueOrDefault(1),  // Default to 1 if null
                criticality.GetValueOrDefault(1)  // Default to 1 if null
            );
        }

        private void ApplyRiskFormatting(IXLRange riskRange)
        {
            // Low Risk (Green)
            riskRange.AddConditionalFormat()
                .WhenBetween(1, 16)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#92D050"));
            
            // Medium Risk (Yellow)
            riskRange.AddConditionalFormat()
                .WhenBetween(17, 48)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#FFFF00"));
            
            // High Risk (Orange)
            riskRange.AddConditionalFormat()
                .WhenBetween(49, 160)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#FFC000"));
            
            // Extreme Risk (Red)
            riskRange.AddConditionalFormat()
                .WhenGreaterThan(160)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#FF0000"));
        }


        public void DownloadAMPModel(
            List<Asset> assetData,
            List<ClassData> classData,
            List<HierarchyItem> hierarchyData,
            List<Expenditure> expenditures,
            List<MaintenanceStrategy> maintenanceStrategies,
            List<MaintenanceProcedure> maintenanceProcedures,
            DateTime startDate,
            DateTime endDate,
            string filePath,
            IProgress<int> progress)
        {
            using (var workbook = new XLWorkbook())
            {
                // Add required sheets using passed parameters
                _workbookSheetService.AddClassDataSheet(workbook, classData);
                _workbookSheetService.AddMaintenanceStrategiesSheet(workbook, maintenanceStrategies);
                _workbookSheetService.AddMaintenanceProceduresSheet(workbook, maintenanceProcedures);

                var worksheet = workbook.Worksheets.Add("AMP Model");
                progress?.Report(10);

                // Set outline buttons to appear on the left
                worksheet.Outline.SummaryVLocation = XLOutlineSummaryVLocation.Top;
                worksheet.Outline.SummaryHLocation = XLOutlineSummaryHLocation.Left;

                // Add hierarchy headers in A3 and B3
                worksheet.Cell("A3").Value = "Description";
                worksheet.Cell("B3").Value = "ID";
                
                // Format headers
                var headerRange = worksheet.Range("A3:B3");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                
                // Set consistent font for hierarchy
                worksheet.Style.Font.FontName = "Helvetica";
                worksheet.Style.Font.FontSize = 11;
                
                progress?.Report(20);

                // Add hierarchy data starting from row 4
                int currentRow = 4;
                if (hierarchyData != null && hierarchyData.Any())
                {
                    foreach (var item in hierarchyData.Where(i => string.IsNullOrEmpty(i.Parent_ID)))
                    {
                        AddHierarchyToExcel(worksheet, item, "", true, ref currentRow);
                    }
                }

                progress?.Report(40);

                // Format hierarchy columns
                worksheet.Column(1).Width = 80;  // Description column
                worksheet.Column(2).Width = 20;  // ID column
                
                // Set hierarchy formatting
                var hierarchyRange = worksheet.Range(4, 1, currentRow - 1, 2);
                hierarchyRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                hierarchyRange.Style.Alignment.WrapText = false;
                
                // Remove any default padding and borders
                worksheet.Style.Border.InsideBorder = XLBorderStyleValues.None;
                worksheet.Style.Border.OutsideBorder = XLBorderStyleValues.None;

                // Set exact row height for all rows
                for (int i = 4; i < currentRow; i++)
                {
                    worksheet.Row(i).Height = 15;
                }

                // Apply grouping to hierarchy
                ApplyHierarchyGrouping(worksheet, currentRow - 1);

                progress?.Report(60);

                // Setup AMP specific content using passed parameters
                SetupAMPContent(worksheet, startDate, endDate, currentRow);
                
                progress?.Report(70);

                // Populate AMP data using passed parameters
                PopulateAMPData(worksheet, assetData, classData, expenditures, startDate, endDate, currentRow);
                
                progress?.Report(80);

                // Add non-asset Capital Projects using passed parameters
                currentRow = AddNonAssetCapitalProjects(worksheet, expenditures, startDate, endDate, currentRow);
                
                progress?.Report(90);

                // Final formatting
                worksheet.Columns().AdjustToContents();
                
                // Save the workbook
                workbook.SaveAs(filePath);
                progress?.Report(100);
            }
        }

        private void SetupAMPContent(IXLWorksheet worksheet, DateTime startDate, DateTime endDate, int startRow)
        {
            SetupAMPWorksheet(worksheet, startDate, endDate);
        }
        private void SetupAMPWorksheet(IXLWorksheet worksheet, DateTime startDate, DateTime endDate)
        {
            // Set white font color for rows 1-3
            worksheet.Rows(1, 3).Style.Font.FontName = "Helvetica";
            worksheet.Rows(1, 3).Style.Font.FontColor = XLColor.White;
            // Set alignment for merged cells in rows 1-2
            worksheet.Rows(1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Rows(1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            // Equipment Details Section
            string[] equipmentHeaders = new[] {
                "Description", "ID", "UnderWarranty", "WarrantyStartDate", "WarrantyEndDate",
                "StdLife", "StdCost", "ClassCode", "MaintenanceStrategyCode", "MaintenanceType",
                "PurchaseCost", "AcqDate", "ConditionRating", "Comment", "Note"
            };
            for (int i = 0; i < equipmentHeaders.Length; i++)
            {
                worksheet.Cell(3, i + 1).Value = equipmentHeaders[i];
            }
            
            var equipmentRange = worksheet.Range("A1:O2");
            equipmentRange.Merge();
            equipmentRange.Value = "Equipment Details";
            equipmentRange.Style.Font.FontSize = 12;
            worksheet.Range("A1:O3").Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC549");
            
            // Group columns C to O
            worksheet.Columns("C:O").Group();

            // Criticality Section
            string[] criticalityHeaders = new[] {
                "Injury", "Environmental", "BusinessContinuity", "Reputation",
                "LossImpactOnTheCompany", "HighestCriticality"
            };
            for (int i = 0; i < criticalityHeaders.Length; i++)
            {
                worksheet.Cell(3, i + 16).Value = criticalityHeaders[i];  // Starting at column P (16)
            }
            
            var criticalityRange = worksheet.Range("P1:U2");
            criticalityRange.Merge();
            criticalityRange.Value = "Criticality";
            criticalityRange.Style.Font.FontSize = 12;
            worksheet.Range("P1:U3").Style.Fill.BackgroundColor = XLColor.FromHtml("#12A8B2");
            // Group Criticality columns Q to U
            worksheet.Columns("Q:U").Group();
            // Calculate project years
            int projectYears = endDate.Year - startDate.Year + 1;
            int currentColumn = 22; // Starting at column V

            // Setup all sections with grouping
            SetupYearlySection(worksheet, ref currentColumn, startDate, projectYears,
                "Remaining Useful Life (years)", "#5B943E", true);

            SetupYearlySection(worksheet, ref currentColumn, startDate, projectYears,
                "Likelihood Forecast (Based on Remaining Life % Score)\nResets to 80% of StdLife at Refurbishment", 
                "#8ED973", true, true);

            SetupYearlySection(worksheet, ref currentColumn, startDate, projectYears,
                "Overall Risk Rating Forecast\nCalculated from Criticality and Likelihood", 
                "#82E37B", true, true);

            SetupYearlySection(worksheet, ref currentColumn, startDate, projectYears,
                "OPEX p.a.\nPreventative Maintenance", "#287DA0", true, true);

            SetupYearlySection(worksheet, ref currentColumn, startDate, projectYears,
                "OPEX p.a.\nCorrective Maintenance", "#44B3E1", true, true);

            SetupYearlySection(worksheet, ref currentColumn, startDate, projectYears,
                "CAPEX p.a.\nReplacement", "#00365E", true, true);

            SetupYearlySection(worksheet, ref currentColumn, startDate, projectYears,
                "CAPEX p.a.\nRefurbishment", "#114973", true, true);

            SetupYearlySection(worksheet, ref currentColumn, startDate, projectYears,
                "CAPEX p.a.\nCapital Project Expenditure", "#003366", true, true);
        }



        private void SetupYearlySection(IXLWorksheet worksheet, ref int startColumn, DateTime startDate, 
            int projectYears, string headerText, string hexColor, bool groupColumns, bool excludeFirstYear = true)
        {
            // Add year headers
            for (int i = 0; i < projectYears; i++)
            {
                worksheet.Cell(3, startColumn + i).Value = startDate.AddYears(i).Year;
            }

            // Merge header cells and set alignment
            var headerRange = worksheet.Range(1, startColumn, 2, startColumn + projectYears - 1);
            headerRange.Merge();
            headerRange.Value = headerText;
            headerRange.Style.Font.FontSize = 12;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            headerRange.Style.Alignment.WrapText = true;

            // Fill color
            worksheet.Range(1, startColumn, 3, startColumn + projectYears - 1)
                .Style.Fill.BackgroundColor = XLColor.FromHtml(hexColor);

            // Group columns if requested, excluding the first year
            if (groupColumns && excludeFirstYear && projectYears > 1)
            {
                worksheet.Columns(startColumn + 1, startColumn + projectYears - 1).Group();
            }

            // Update current column position
            startColumn += projectYears;
        }

        private void PopulateAMPData(IXLWorksheet worksheet, List<Asset> assetData, List<ClassData> classData, 
            List<Expenditure> expenditures, DateTime startDate, DateTime endDate, int startRow)
        {
            int projectYears = endDate.Year - startDate.Year + 1;
            var assetLookup = assetData.ToDictionary(a => a.Asset_ID, a => a);
            var classLookup = classData.ToDictionary(c => c.HierarchyCode, c => c);

            // Group expenditures by Asset_ID
            var expendituresByAsset = expenditures
                .Where(e => !string.IsNullOrEmpty(e.Asset_ID))
                .GroupBy(e => e.Asset_ID)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var asset in assetData)
            {
                if (expendituresByAsset.TryGetValue(asset.Asset_ID, out var assetExpenditures))
                {
                    // Group expenditures by type and year
                    var groupedExpenditures = assetExpenditures
                        .GroupBy(e => new { e.ExpenditureType, e.ExpenditureYear })
                        .ToDictionary(g => g.Key, g => g.Sum(e => e.ExpenditureValue));

                    // Populate expenditure values
                    foreach (var exp in groupedExpenditures)
                    {
                        int yearOffset = exp.Key.ExpenditureYear - startDate.Year;
                        int columnOffset = GetExpenditureTypeOffset(exp.Key.ExpenditureType, projectYears);
                        
                        if (columnOffset >= 0 && yearOffset >= 0 && yearOffset < projectYears)
                        {
                            worksheet.Cell(startRow, 10 + columnOffset + yearOffset).Value = exp.Value;
                        }
                    }
                }
            }
        }

        private int GetExpenditureTypeOffset(string expenditureType, int projectYears)
        {
            return expenditureType?.ToLower() switch
            {
                "maintenance" => 0,
                "refurbishment" => projectYears,
                "replacement" => projectYears * 2,
                "disposal" => projectYears * 3,
                "capital" => projectYears * 4,
                "operating" => projectYears * 5,
                _ => -1
            };
        }

        

        // Helper method to format cells consistently
        private void ApplyStandardFormatting(IXLRange range, string backgroundColor, bool isHeader = false)
        {
            range.Style.Fill.BackgroundColor = XLColor.FromHtml(backgroundColor);
            range.Style.Font.FontColor = XLColor.White;
            
            if (isHeader)
            {
                range.Style.Font.FontSize = 12;
                range.Style.Font.Bold = true;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                range.Style.Alignment.WrapText = true;
            }
            else
            {
                range.Style.Font.FontSize = 11;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }

        private void ApplyAMPFormatting(IXLWorksheet worksheet, int lastColumn, int lastRow)
        {
            var dataRange = worksheet.Range(3, 2, lastRow, lastColumn);
            
            // Set borders
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            
            // Set font
            dataRange.Style.Font.FontName = "Helvetica";
            dataRange.Style.Font.FontSize = 11;
            dataRange.Style.Font.FontColor = XLColor.Black;
            
            // Set fill color
            dataRange.Style.Fill.BackgroundColor = XLColor.White;

            // Add filters to row 3
            var filterRow = worksheet.Range(3, 2, 3, lastColumn);
            filterRow.SetAutoFilter();

            // Format numbers in the data range
            var numericColumns = worksheet.Columns()
                .Where(col => col.Cells().Any(cell => cell.Value is double || cell.Value is int || cell.Value is decimal))
                .ToList();

            foreach (var column in numericColumns)
            {
                column.Style.NumberFormat.Format = "#,##0";
            }
        }

        // Helper method to add year headers
        private void AddYearHeaders(IXLWorksheet worksheet, int startColumn, int projectYears, DateTime projectStartDate, string backgroundColor)
        {
            for (int i = 0; i < projectYears; i++)
            {
                var cell = worksheet.Cell(3, startColumn + i);
                cell.Value = projectStartDate.Year + i;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(backgroundColor);
                cell.Style.Font.FontColor = XLColor.White;
            }
        }

        // Helper method to get Excel column letter
        private string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnName;
        }

        public void SaveHierarchyToFile(List<HierarchyItem> hierarchyData, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Hierarchy");
                
                // Add headers
                worksheet.Cell(1, 1).Value = "Description";
                worksheet.Cell(1, 2).Value = "ID";
                
                // Format headers
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRow.Height = 20;  // Header can be slightly taller

                // Set consistent font and size for the entire worksheet
                worksheet.Style.Font.FontName = "Helvetica";
                worksheet.Style.Font.FontSize = 11;
                
                // Remove any default padding
                worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                worksheet.Style.Alignment.WrapText = false;

                // Add data with proper indentation and tree structure
                int row = 2;
                foreach (var item in hierarchyData.Where(i => string.IsNullOrEmpty(i.Parent_ID)))
                {
                    AddHierarchyToExcel(worksheet, item, "", true, ref row);
                }

                // Apply grouping
                ApplyHierarchyGrouping(worksheet, row - 1);

                // Format worksheet
                worksheet.Column(1).Width = 80;
                worksheet.Column(2).Width = 20;
                
                // Ensure no gaps between rows
                worksheet.Style.Border.InsideBorder = XLBorderStyleValues.None;
                worksheet.Style.Border.OutsideBorder = XLBorderStyleValues.None;

                workbook.SaveAs(filePath);
            }
        }

        private void ApplyHierarchyGrouping(IXLWorksheet worksheet, int lastRow)
        {
            // Get the maximum depth of the hierarchy
            int maxLevel = 0;
            for (int i = 2; i <= lastRow; i++)
            {
                string cellValue = worksheet.Cell(i, 1).GetString();
                int level = (cellValue.Count(c => c == '│') + (cellValue.Contains("└──") || cellValue.Contains("├──") ? 1 : 0));
                maxLevel = Math.Max(maxLevel, level);
            }

            // Apply grouping from deepest to shallowest level
            for (int level = maxLevel; level >= 0; level--)
            {
                int? groupStart = null;
                for (int i = 2; i <= lastRow; i++)
                {
                    string cellValue = worksheet.Cell(i, 1).GetString();
                    int currentLevel = (cellValue.Count(c => c == '│') + (cellValue.Contains("└──") || cellValue.Contains("├──") ? 1 : 0));

                    if (currentLevel == level && !groupStart.HasValue)
                    {
                        groupStart = i;
                    }
                    else if (groupStart.HasValue && (currentLevel < level || i == lastRow))
                    {
                        int groupEnd = i == lastRow ? i : i - 1;
                        worksheet.Rows(groupStart.Value, groupEnd).Group();
                        worksheet.Row(groupStart.Value).Collapse();
                        groupStart = null;
                    }
                }
            }
        }

        private int FindGroupEnd(IXLWorksheet worksheet, int startRow, int lastRow, int level)
        {
            for (int i = startRow + 1; i <= lastRow; i++)
            {
                string cellValue = worksheet.Cell(i, 1).GetString();
                int currentLevel = (cellValue.Length - cellValue.TrimStart().Length) / 2;
                if (currentLevel <= level)
                {
                    return i - 1;
                }
            }
            return lastRow;
        }

        private void AddHierarchyToExcel(IXLWorksheet worksheet, HierarchyItem item, string indent, bool isLast, ref int row)
        {
            string treeChar = isLast ? "└" : "├";
            string lineIndent = indent + treeChar + "───";
            
            // Add extra spacing before the description
            worksheet.Cell(row, 1).Value = lineIndent + " " + item.Description;
            worksheet.Cell(row, 2).Value = item.ID;
            
            // Set exact row height
            worksheet.Row(row).Height = 15;
            
            // Ensure vertical alignment and no padding
            worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            
            row++;
            
            if (item.Children.Any())
            {
                for (int i = 0; i < item.Children.Count; i++)
                {
                    string newIndent = indent + (isLast ? "    " : "│   ");
                    AddHierarchyToExcel(worksheet, item.Children[i], newIndent, i == item.Children.Count - 1, ref row);
                }
            }
        }

        private int GetHierarchyLevel(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return id.Count(c => c == '-');
        }

        private IEnumerable<HierarchyItem> FlattenHierarchy(List<HierarchyItem> items)
        {
            foreach (var item in items)
            {
                yield return item;
                foreach (var child in FlattenHierarchy(item.Children))
                {
                    yield return child;
                }
            }
        }

        private IEnumerable<HierarchyItem> FlattenHierarchyWithLevels(List<HierarchyItem> items, int level = 0)
        {
            foreach (var item in items)
            {
                item.Level = level;
                yield return item;
                foreach (var child in FlattenHierarchyWithLevels(item.Children, level + 1))
                {
                    yield return child;
                }
            }
        }

        

    }
}

