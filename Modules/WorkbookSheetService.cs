using ClosedXML.Excel;
using CoolMangoes.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace CoolMangoes.Modules
{
    public class WorkbookSheetService
    {
        private void AddHomeButton(IXLWorksheet worksheet)
        {
            var buttonRange = worksheet.Range("A2:A3");
            buttonRange.Merge();
            buttonRange.Value = "Home";
            buttonRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            buttonRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            buttonRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            
            // Add hyperlink to Home sheet
            worksheet.Cell("A2").SetHyperlink(new XLHyperlink("Home!A1"));
        }

        public void AddHomeSheet(IXLWorkbook workbook, DateTime startDate, DateTime endDate)
        {
            var worksheet = workbook.Worksheets.Add("Home");
            
            // Set default styles
            worksheet.Style.Font.FontName = "Helvetica";
            worksheet.Style.Font.FontSize = 9;
            worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            
            // Set column widths
            worksheet.Column("A").Width = 25;
            worksheet.Column("B").Width = 45;

            // Add title and description
            var titleRange = worksheet.Range("A3:B3");
            titleRange.Merge();
            titleRange.Value = "About\nThis tool calculates the expenditure required to maintain XXXXXXXXXXX.";
            titleRange.Style.Alignment.WrapText = true;

            // Add year information
            worksheet.Cell("A4").Value = "Model start year:";
            worksheet.Cell("B4").Value = startDate.Year;
            worksheet.Cell("A5").Value = "Model end year:";
            worksheet.Cell("B5").Value = endDate.Year;

            // Define navigation buttons and descriptions
            var navigationItems = new Dictionary<string, (string Sheet, string Description)>
            {
                {"Equipment Data", ("EquipmentData", "This sheet contains the list of assets that will be included in the expenditure plan.")},
                {"Classification Data", ("ClassData", "List of asset classification, this is where maintenance strategies are assigned and life cycle information is held.")},
                {"Expenditure Plan", ("Expenditure", "Detailed outline of anticipated expenses associated with each asset in the asset list.")},
                {"Expenditure Report", ("ExpenditureReport", "Reporting yearly forecast on capital and operational expenditure.")},
                {"Maintenance Strategies", ("MaintenanceStrategies", "List of maintenance strategies and their hourly cost.")},
                {"Maintenance Procedures", ("MaintenanceProcedures", "List of maintenance procedures associated with maintenance strategies.")}
            };

            int currentRow = 7;
            foreach (var item in navigationItems)
            {
                // Add button with hyperlink
                var buttonCell = worksheet.Cell($"A{currentRow}");
                buttonCell.Value = item.Key;
                buttonCell.SetHyperlink(new XLHyperlink($"{item.Value.Sheet}!A1"));
                buttonCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Add description
                var descCell = worksheet.Cell($"B{currentRow}");
                descCell.Value = item.Value.Description;
                descCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                descCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                descCell.Style.Alignment.WrapText = true;

                currentRow++;
            }

            // Add logo
            try
            {
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"obj\Covaris-Logo-Light.png");
                if (File.Exists(logoPath))
                {
                    var picture = worksheet.AddPicture(logoPath)
                        .MoveTo(worksheet.Cell("B1"))
                        .WithSize(404, 62); // Size in pixels (4.04cm ≈ 404px, 0.62cm ≈ 62px)
                }
            }
            catch (Exception ex)
            {
                // Log error or handle missing logo gracefully
                Console.WriteLine($"Error adding logo: {ex.Message}");
            }

            // Set white background for the entire used range
            worksheet.Range("A1:B13").Style.Fill.BackgroundColor = XLColor.White;

            worksheet.Range("A7:B12").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range("A7:B12").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        public void AddEquipmentDataSheet(IXLWorkbook workbook, List<Asset> assetData)
        {
            var worksheet = workbook.Worksheets.Add("EquipmentData");
            
            // Default styles
            worksheet.Style.Font.FontName = "Helvetica";
            worksheet.Style.Font.FontSize = 11;
            worksheet.Style.Font.FontColor = XLColor.Black;
            worksheet.Style.Fill.BackgroundColor = XLColor.White;

            // Add Home button
            AddHomeButton(worksheet);

            // Add description
            var descRange = worksheet.Range("A4:D5");
            descRange.Merge();
            descRange.Value = "Below is the asset list for XXXXX that will be included in the expenditure plan.";
            descRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            descRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Define section headers and their properties
            var sections = new[]
            {
                new {
                    Title = "Location Information",
                    StartColumn = "A",
                    EndColumn = "D",
                    Color = "#00365E",
                    TextColor = XLColor.White,
                    Columns = new[] { "Location1", "Location2", "Location3", "Location4" }
                },
                new {
                    Title = "Equipment",
                    StartColumn = "E",
                    EndColumn = "G",
                    Color = "#D4E1E8",
                    TextColor = XLColor.Black,
                    Columns = new[] { "Asset_ID", "AssetDescription", "Parent_ID" }
                },
                new {
                    Title = "Classification Data",
                    StartColumn = "H",
                    EndColumn = "M",
                    Color = "#114973",
                    TextColor = XLColor.White,
                    Columns = new[] { "AssetHierarchy", "HierarchyL1", "HierarchyL2", "HierarchyL3", "HierarchyL4", "HierarchyCode" }
                },
                new {
                    Title = "Equipment Data",
                    StartColumn = "N",
                    EndColumn = "U",
                    Color = "#3DADA2",
                    TextColor = XLColor.Black,
                    Columns = new[] { "Manufacturer", "ModelNumber", "ManufSerialNo", "AcqDate", "PurchaseCost", "UnderWarranty", "WarrantyStartDate", "WarrantyEndDate" }
                },
                new {
                    Title = "Life Cycle Details",
                    StartColumn = "V",
                    EndColumn = "X",
                    Color = "#F06350",
                    TextColor = XLColor.Black,
                    Columns = new[] { "ConditionRating", "CurrentUsage", "OperatingEnvironment" }
                },
                new {
                    Title = "Maintenance Information",
                    StartColumn = "Y",
                    EndColumn = "AC",
                    Color = "#D4E1E8",
                    TextColor = XLColor.Black,
                    Columns = new[] { "ObservationDate", "PlannedStartDate", "MaintenanceStrategyCode", "MaintenanceType", "Statutory" }
                },
                new {
                    Title = "Criticality",
                    StartColumn = "AD",
                    EndColumn = "AI",
                    Color = "#12A8B2",
                    TextColor = XLColor.White,
                    Columns = new[] { "Injury", "Environmental", "BusinessContinuity", "Reputation", "LossImpactOnTheCompany", "HighestCriticality" }
                }
            };

            // Add section headers and format them
            foreach (var section in sections)
            {
                // Merge and set section title
                var headerRange = worksheet.Range($"{section.StartColumn}8:{section.EndColumn}8");
                headerRange.Merge();
                headerRange.Value = section.Title;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontSize = 12;
                headerRange.Style.Font.FontColor = section.TextColor;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(section.Color);
                
                // Set background color for entire section
                var sectionRange = worksheet.Range($"{section.StartColumn}8:{section.EndColumn}9");
                sectionRange.Style.Fill.BackgroundColor = XLColor.FromHtml(section.Color);
                
                // Add column headers
                var startCol = XLHelper.GetColumnNumberFromLetter(section.StartColumn);
                for (int i = 0; i < section.Columns.Length; i++)
                {
                    var cell = worksheet.Cell(9, startCol + i);
                    cell.Value = section.Columns[i];
                    cell.Style.Font.FontSize = 12;
                    cell.Style.Font.FontColor = section.TextColor;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml(section.Color);
                }

                // Add borders
                var fullRange = worksheet.Range($"{section.StartColumn}8:{section.EndColumn}9");
                fullRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                fullRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Add asset data
            int currentRow = 10;
            foreach (var asset in assetData)
            {
                int currentCol = 1;
                
                // Location Information
                worksheet.Cell(currentRow, currentCol++).Value = asset.Location1;
                worksheet.Cell(currentRow, currentCol++).Value = asset.Location2;
                worksheet.Cell(currentRow, currentCol++).Value = asset.Location3;
                worksheet.Cell(currentRow, currentCol++).Value = asset.Location4;
                
                // Equipment
                worksheet.Cell(currentRow, currentCol++).Value = asset.Asset_ID;
                worksheet.Cell(currentRow, currentCol++).Value = asset.AssetDescription;
                worksheet.Cell(currentRow, currentCol++).Value = asset.Parent_ID;
                
                // Classification Data
                worksheet.Cell(currentRow, currentCol++).Value = asset.AssetHierarchy;
                worksheet.Cell(currentRow, currentCol++).Value = asset.HierarchyL1;
                worksheet.Cell(currentRow, currentCol++).Value = asset.HierarchyL2;
                worksheet.Cell(currentRow, currentCol++).Value = asset.HierarchyL3;
                worksheet.Cell(currentRow, currentCol++).Value = asset.HierarchyL4;
                worksheet.Cell(currentRow, currentCol++).Value = asset.HierarchyCode;

                // Equipment Data
                worksheet.Cell(currentRow, currentCol++).Value = asset.Manufacturer;
                worksheet.Cell(currentRow, currentCol++).Value = asset.ModelNumber;
                worksheet.Cell(currentRow, currentCol++).Value = asset.ManufSerialNo;
                worksheet.Cell(currentRow, currentCol++).Value = asset.AcqDate;
                worksheet.Cell(currentRow, currentCol++).Value = asset.PurchaseCost;

                // Life Cycle Details
                worksheet.Cell(currentRow, currentCol++).Value = asset.ConditionRating;
                worksheet.Cell(currentRow, currentCol++).Value = asset.CurrentUsage;
                worksheet.Cell(currentRow, currentCol++).Value = asset.OperatingEnvironment;    

                // Maintenance Information
                worksheet.Cell(currentRow, currentCol++).Value = asset.ObservationDate;
                worksheet.Cell(currentRow, currentCol++).Value = asset.PlannedStartDate;
                worksheet.Cell(currentRow, currentCol++).Value = asset.MaintenanceStrategyCode;
                worksheet.Cell(currentRow, currentCol++).Value = asset.MaintenanceType;
                worksheet.Cell(currentRow, currentCol++).Value = asset.Statutory;

                // Criticality
                worksheet.Cell(currentRow, currentCol++).Value = asset.Injury;
                worksheet.Cell(currentRow, currentCol++).Value = asset.Environmental;
                worksheet.Cell(currentRow, currentCol++).Value = asset.BusinessContinuity;
                worksheet.Cell(currentRow, currentCol++).Value = asset.Reputation;
                worksheet.Cell(currentRow, currentCol++).Value = asset.LossImpactOnTheCompany;
                worksheet.Cell(currentRow, currentCol++).Value = asset.HighestCriticality;

                // Add borders to data row
                var dataRange = worksheet.Range($"A{currentRow}:AI{currentRow}");
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                
                currentRow++;
            }
            // Add borders to entire data range
            if (currentRow > 10)
            {
                var entireRange = worksheet.Range($"A8:AI{currentRow - 1}");
                entireRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                entireRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
        }

        public void AddClassDataSheet(IXLWorkbook workbook, List<ClassData> classData)
        {
            var worksheet = workbook.Worksheets.Add("ClassData");
            
            AddHomeButton(worksheet);
            // Set default style
            worksheet.Style.Font.FontName = "Helvetica";
            worksheet.Style.Font.FontSize = 11;
            worksheet.Style.Fill.BackgroundColor = XLColor.White;

            // Add merged description
            var descRange = worksheet.Range("A4:C6");
            descRange.Merge();
            descRange.Value = "Below are a list of asset classifications, against each asset classification a maintenance strategy can be assigned here. The remaining columns influence the frequency and cost of asset replacements and renewals.";
            descRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            descRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            descRange.Style.Alignment.WrapText = true;

            // Define headers
            var headers = new Dictionary<string, (string Column, string Color)>
            {
                {"AssetHierarchy", ("A", "#00365E")},
                {"HierarchyCode", ("B", "#00365E")},
                {"AssetType", ("C", "#00365E")},
                {"MaintenanceType", ("D", "#00365E")},
                {"Statutory", ("E", "#00365E")},
                {"EstimatedLife", ("F", "#00365E")},
                {"RefurbishmentFrequency", ("G", "#00365E")},
                {"RefurbishmentCostAsProportionOfReplacementCost", ("H", "#00365E")},
                {"MinCost", ("I", "#00365E")},
                {"MaxCost", ("J", "#00365E")},
                {"AvgReplacementCost", ("K", "#00365E")},
                {"Injury", ("L", "#12A8B2")},
                {"Environmental", ("M", "#12A8B2")},
                {"BusinessContinuity", ("N", "#12A8B2")},
                {"Reputation", ("O", "#12A8B2")},
                {"LossImpactOnTheCompany", ("P", "#12A8B2")},
                {"HighestCriticality", ("Q", "#12A8B2")}
            };

            // Add headers
            foreach (var header in headers)
            {
                var cell = worksheet.Cell($"{header.Value.Column}9");
                cell.Value = header.Key;
                cell.Style.Font.FontSize = header.Value.Column.CompareTo("L") >= 0 ? 12 : 11;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(header.Value.Color);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Add data
            int currentRow = 10;
            foreach (var item in classData)
            {
                worksheet.Cell($"A{currentRow}").Value = item.AssetHierarchy;
                worksheet.Cell($"B{currentRow}").Value = item.HierarchyCode;
                worksheet.Cell($"C{currentRow}").Value = item.AssetType;
                worksheet.Cell($"D{currentRow}").Value = item.MaintenanceType;
                worksheet.Cell($"E{currentRow}").Value = item.Statutory;
                worksheet.Cell($"F{currentRow}").Value = item.EstimatedLife;
                worksheet.Cell($"G{currentRow}").Value = item.RefurbishmentFrequency;
                worksheet.Cell($"H{currentRow}").Value = item.RefurbishmentCostAsProportionOfReplacementCost;
                worksheet.Cell($"I{currentRow}").Value = item.MinCost;
                worksheet.Cell($"J{currentRow}").Value = item.MaxCost;
                worksheet.Cell($"K{currentRow}").Value = item.AvgReplacementCost;
                worksheet.Cell($"L{currentRow}").Value = item.Injury;
                worksheet.Cell($"M{currentRow}").Value = item.Environmental;
                worksheet.Cell($"N{currentRow}").Value = item.BusinessContinuity;
                worksheet.Cell($"O{currentRow}").Value = item.Reputation;
                worksheet.Cell($"P{currentRow}").Value = item.LossImpactOnTheCompany;
                worksheet.Cell($"Q{currentRow}").Value = item.HighestCriticality;
                
                // Add borders to data row
                worksheet.Range($"A{currentRow}:Q{currentRow}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range($"A{currentRow}:Q{currentRow}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                currentRow++;
            }
            // Add for entire range at the end
            if (currentRow > 10)
            {
                var entireRange = worksheet.Range($"A9:Q{currentRow - 1}");
                entireRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                entireRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        public void AddMaintenanceStrategiesSheet(IXLWorkbook workbook, List<MaintenanceStrategy> strategies)
        {
            var worksheet = workbook.Worksheets.Add("MaintenanceStrategies");
            worksheet.Style.Fill.BackgroundColor = XLColor.White;

            AddHomeButton(worksheet);

            // Add merged description
            var descRange = worksheet.Range("A4:B6");
            descRange.Merge();
            descRange.Value = "Below is a list of maintenance strategies and associated hourly cost. Their procedures are on the \"Maintenance Procedures\" sheet.";
            descRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            descRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            descRange.Style.Alignment.WrapText = true;

            // Add headers
            string[] headers = { "StrategyCode", "StrategyDescription", "CostPerHour", "ResourceType", "ResourceName" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(9, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.FontSize = 12;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#00365E");
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Add data
            int currentRow = 10;
            foreach (var strategy in strategies)
            {
                worksheet.Cell($"A{currentRow}").Value = strategy.StrategyCode;
                worksheet.Cell($"B{currentRow}").Value = strategy.StrategyDescription;
                worksheet.Cell($"C{currentRow}").Value = strategy.CostPerHour;
                worksheet.Cell($"D{currentRow}").Value = strategy.ResourceType;
                worksheet.Cell($"E{currentRow}").Value = strategy.ResourceName;

                worksheet.Range($"A{currentRow}:E{currentRow}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range($"A{currentRow}:E{currentRow}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                currentRow++;
            }

            // Add for entire range at the end
            if (currentRow > 10)
            {
                var entireRange = worksheet.Range($"A9:E{currentRow - 1}");
                entireRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                entireRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        public void AddMaintenanceProceduresSheet(IXLWorkbook workbook, List<MaintenanceProcedure> procedures)
        {
            var worksheet = workbook.Worksheets.Add("MaintenanceProcedures");
            worksheet.Style.Fill.BackgroundColor = XLColor.White;

            AddHomeButton(worksheet);

            // Add merged description
            var descRange = worksheet.Range("A4:D6");
            descRange.Merge();
            descRange.Value = "Below is a list of maintenance procedures associated with maintenance strategies for each asset class in the equipment list.";
            descRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            descRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            descRange.Style.Alignment.WrapText = true;

            // Define headers
            string[] headers = {
                "StrategyCode", "StrategyDescription", "HierarchyL1", "HierarchyL2",
                "HierarchyL3", "HierarchyL4", "ProcedureCode", "ProcedureDescription",
                "Duration", "Frequency", "FrequencyType", "MaintenanceStatus",
                "Statutory", "LastDoneDate"
            };

            // Add headers
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(9, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.FontSize = 12;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#00365E");
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Add data
            int currentRow = 10;
            foreach (var procedure in procedures)
            {
                worksheet.Cell($"A{currentRow}").Value = procedure.StrategyCode;
                worksheet.Cell($"B{currentRow}").Value = procedure.StrategyDescription;
                worksheet.Cell($"C{currentRow}").Value = procedure.HierarchyL1;
                worksheet.Cell($"D{currentRow}").Value = procedure.HierarchyL2;
                worksheet.Cell($"E{currentRow}").Value = procedure.HierarchyL3;
                worksheet.Cell($"F{currentRow}").Value = procedure.HierarchyL4;
                worksheet.Cell($"G{currentRow}").Value = procedure.ProcedureCode;
                worksheet.Cell($"H{currentRow}").Value = procedure.ProcedureDescription;
                worksheet.Cell($"I{currentRow}").Value = procedure.Duration;
                worksheet.Cell($"J{currentRow}").Value = procedure.Frequency;
                worksheet.Cell($"K{currentRow}").Value = procedure.FrequencyType;
                worksheet.Cell($"L{currentRow}").Value = procedure.MaintenanceStatus;
                worksheet.Cell($"M{currentRow}").Value = procedure.Statutory;
                worksheet.Cell($"N{currentRow}").Value = procedure.LastDoneDate;

                worksheet.Range($"A{currentRow}:N{currentRow}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range($"A{currentRow}:N{currentRow}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                currentRow++;
            } 
            // Add for entire range at the end    
            if (currentRow > 10)
            {
                var entireRange = worksheet.Range($"A9:N{currentRow - 1}");
                entireRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                entireRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }   
        }
    }
}