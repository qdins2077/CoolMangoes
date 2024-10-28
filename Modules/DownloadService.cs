using System;
using System.IO;
using System.Collections.Generic;
using ClosedXML.Excel;  // Using ClosedXML for Excel creation
using CoolMangoes.Models;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Threading.Tasks;
//ExcelPackage.LicenseContext = LicenseContext.NonCommercial;



namespace CoolMangoes.Modules
{
    public class DownloadService
    {
        private readonly string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        public void DownloadAssetDataTemplate()
        {
            string fileName = "AssetDataTemplate.csv";
            string filePath = Path.Combine(downloadsFolder, fileName);
            string csvHeadings = "Location1,Location2,Location3,Location4,Asset_ID,Parent_ID,AssetDescription,AssetHierarchy,HierarchyL1,HierarchyL2,HierarchyL3,HierarchyL4,HierarchyCode,Manufacturer,ModelNumber,ManufSerialNo,AcqDate,ConditionRating,CurrentUsage,OperatingEnvironment,PurchaseCost,ObservationDate,MaintenanceStrategyCode,MaintenanceType,Statutory,Criticality,PlannedStartDate,PlannedEndDate";

            WriteCsvTemplate(filePath, csvHeadings);
        }

        public void DownloadClassDataTemplate()
        {
            string fileName = "ClassDataTemplate.csv";
            string filePath = Path.Combine(downloadsFolder, fileName);
            string csvHeadings = "AssetHierarchy,HierarchyCode,AssetType,MaintenanceType,Statutory,EstimatedLife,RefurbishmentFrequency,RefurbishmentCostAsProportionOfReplacementCost,MinCost,MaxCost,AvgReplacementCost";

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

     

        public void DownloadLCCCostModel(IEnumerable<Expenditure> expenditureData, string filePath, IProgress<int> progress = null)
        {
            if (expenditureData == null)
            {
                throw new ArgumentNullException(nameof(expenditureData), "Expenditure data cannot be null.");
            }

            int total = expenditureData.Count();
            int completed = 0;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ExpenditurePlan");

                // Set global styles
                worksheet.Style.Font.FontName = "Helvetica";
                worksheet.Style.Font.FontSize = 11; // Default font size
                

                // Set heading row (row 9) font size to 12
                worksheet.Row(9).Style.Font.FontSize = 12;

                // Add auto filter to row 9 from column A to S
                worksheet.Range("A9:S9").SetAutoFilter();

                // Freeze panes (row 10 and below will scroll)
                worksheet.SheetView.FreezeRows(9);

                // Merge cells A5 to D7 and add text
                var mergedCell = worksheet.Range("A5:D7");
                mergedCell.Merge();
                mergedCell.Value = "An expenditure plan is a detailed outline or breakdown of the anticipated expenses associated with each asset or item listed in an asset list. It serves as a guide to estimate and allocate the necessary funds required for acquiring, maintaining, or operating those assets.";
                mergedCell.Style.Alignment.WrapText = true;
                mergedCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                mergedCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // Define headers
                var headers = new string[]
                {
                    "Location1", "Location2", "Location3", "Location4", "Asset_ID", "AssetDescription",
                    "HierarchyL1", "HierarchyL2", "HierarchyL3", "HierarchyL4", "AssetHierarchy", "HierarchyCode",
                    "ExpenditureValue", "AcquisitionDate", "ExpenditureDate","ExpenditureYear", "ExpenditureType", "ExpenditureDescription", "Comment"
                };

                int startingRow = 9;
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(startingRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#00365E");
                    cell.Style.Font.FontColor = XLColor.White;
                }

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
                    worksheet.Cell(row, 14).Value = expenditure.AcqDate.HasValue ? expenditure.AcqDate.Value.ToShortDateString() : "";
                    worksheet.Cell(row, 15).Value = expenditure.ExpenditureDate.ToShortDateString();
                    worksheet.Cell(row, 16).Value = expenditure.ExpenditureDate.Year;
                    worksheet.Cell(row, 17).Value = expenditure.ExpenditureType ?? "";
                    worksheet.Cell(row, 18).Value = expenditure.ExpenditureDescription ?? "";
                    worksheet.Cell(row, 19).Value = expenditure.Comment ?? "";

                    row++;

                    // Increment progress and report it
                    completed++;
                    int progressValue = (completed * 100) / total;
                    progress?.Report(progressValue);
                }

                worksheet.Columns().AdjustToContents();

                // Save the workbook
                try
                {
                    workbook.SaveAs(filePath);
                    Console.WriteLine($"File has been saved successfully at {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving the Excel file: {ex.Message}");
                    throw;
                }
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

    
    }
}

