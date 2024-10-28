using System;
using System.Collections.Generic;
using System.Linq;
using CoolMangoes.Models;

namespace CoolMangoes.Modules
{
    public class ExpenditurePlanService
    {
        private bool _isFlatModeSelected;

        private readonly List<Asset> _assetDataList;
        private readonly Dictionary<string, ClassData> _classDataList;
        private readonly List<MaintenanceProcedure> _procedureDataList;
        private readonly List<MaintenanceStrategy> _maintenanceStrategiesList;
        private readonly DateTime _projectStartDate;
        private readonly DateTime _projectEndDate;
        private readonly List<CapitalProject> _capitalProjectsList;
        private readonly Dictionary<string, List<CapitalProject>> _assetCapitalProjects;

        // Constructor
        public ExpenditurePlanService(
            List<Asset> assetData,
            List<ClassData> classData,
            List<MaintenanceProcedure> procedureData,
            List<MaintenanceStrategy> maintenanceStrategies,
            DateTime projectStartDate,
            DateTime projectEndDate,
            List<CapitalProject> capitalProjectsList,
            bool isFlatModeSelected = false)
        {
            _assetDataList = assetData;
            _procedureDataList = procedureData;
            _maintenanceStrategiesList = maintenanceStrategies;
            _projectStartDate = projectStartDate;
            _projectEndDate = projectEndDate;
            _capitalProjectsList = capitalProjectsList ?? new List<CapitalProject>();

            // Convert classData list to a dictionary for fast lookups by HierarchyCode
            _classDataList = classData
                .Where(entry => !string.IsNullOrWhiteSpace(entry.HierarchyCode))
                .ToDictionary(entry => entry.HierarchyCode);

            // Build a dictionary mapping Asset_ID to capital projects
            _assetCapitalProjects = _capitalProjectsList
                .Where(cp => !string.IsNullOrEmpty(cp.Asset_ID))
                .GroupBy(cp => cp.Asset_ID)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Flat PM mode selected
            _isFlatModeSelected = isFlatModeSelected;
        }

        // Generates the expenditure plan lazily to avoid memory overflow
        public IEnumerable<Expenditure> GenerateExpenditurePlan()
        {
            // Schedule Capital Projects without Asset_ID first
            foreach (var capitalProject in _capitalProjectsList.Where(cp => string.IsNullOrEmpty(cp.Asset_ID)))
            {
                foreach (var expenditure in GenerateCapitalProjectExpenditures(capitalProject))
                {
                    yield return expenditure;
                }
            }

            // Process each asset for regular expenditures
            foreach (var asset in _assetDataList)
            {
                if (string.IsNullOrWhiteSpace(asset.HierarchyCode))
                {
                    Console.WriteLine($"[Warning] Skipping asset {asset.Asset_ID} due to missing HierarchyCode.");
                    continue;
                }

                if (!_classDataList.TryGetValue(asset.HierarchyCode, out var classData))
                {
                    Console.WriteLine($"[Warning] Skipping asset {asset.Asset_ID} due to missing class data.");
                    continue;
                }

                foreach (var expenditure in ProcessAsset(asset, classData))
                {
                    yield return expenditure;
                }
            }

            // Process Preventative Maintenance and generate yearly summaries
            var pmExpenditures = new List<Expenditure>();
            foreach (var asset in _assetDataList)
            {
                foreach (var expenditure in ProcessPreventativeMaintenance(asset))
                {
                    pmExpenditures.Add(expenditure);
                }
            }

            // For LCC Cost Model, only return the yearly summaries
            foreach (var summary in GenerateYearlyPMSummaries(pmExpenditures))
            {
                yield return summary;
            }
        }

        // Add a separate method for PM Schedule that includes all detailed records
        public IEnumerable<Expenditure> GeneratePMSchedule()
        {
            foreach (var asset in _assetDataList)
            {
                foreach (var expenditure in ProcessPreventativeMaintenance(asset))
                {
                    yield return expenditure;
                }
            }
        }

        // Processes an individual asset to generate expenditures
        private IEnumerable<Expenditure> ProcessAsset(Asset asset, ClassData classData)
        {
            var estimatedLife = classData.EstimatedLife ?? 0.0;
            var refurbishmentFrequency = classData.RefurbishmentFrequency ?? 0.0;

            if (estimatedLife <= 0.0 && refurbishmentFrequency <= 0.0)
            {
                Console.WriteLine($"[Warning] Skipping asset {asset.Asset_ID} due to no valid Estimated Life or Refurbishment Frequency.");
                yield break;
            }

            // Always check ConditionRating first
            double conditionRatingValue = asset.ConditionRating ?? 3.0;
            if (conditionRatingValue < 1.0 || conditionRatingValue > 5.0)
            {
                Console.WriteLine($"[Warning] Invalid ConditionRating ({conditionRatingValue}) for asset {asset.Asset_ID}. Defaulting to 3.");
                conditionRatingValue = 3.0;
            }
            int conditionRating = (int)Math.Round(conditionRatingValue);

            // Determine if asset has any Capital Projects
            bool hasCapitalProjects = _assetCapitalProjects.TryGetValue(asset.Asset_ID, out var assetCapitalProjects) && assetCapitalProjects.Any();

            if (hasCapitalProjects)
            {
                // Process based on Capital Projects
                foreach (var expenditure in ProcessAssetWithCapitalProjects(asset, classData, assetCapitalProjects))
                {
                    yield return expenditure;
                }
            }
            else
            {
                // Process based on Acquisition Date or Condition
                foreach (var expenditure in ProcessAssetWithoutCapitalProjects(asset, classData, conditionRating))
                {
                    yield return expenditure;
                }
            }
        }

         // Processes preventative maintenance for an asset
        private IEnumerable<Expenditure> ProcessPreventativeMaintenance(Asset asset)
        {
            var strategyCode = asset.MaintenanceStrategyCode;
            if (string.IsNullOrWhiteSpace(strategyCode))
            {
                yield break;
            }

            var relevantProcedures = _procedureDataList
                .Where(p => p.StrategyCode == strategyCode)
                .ToList();

            var strategy = _maintenanceStrategiesList
                .FirstOrDefault(s => s.StrategyCode == strategyCode);

            if (strategy == null)
            {
                yield break;
            }

            var procedureDates = new Dictionary<DateTime, MaintenanceProcedure>();

            foreach (var procedure in relevantProcedures)
            {
                DateTime nextDate = procedure.LastDoneDate ?? _projectStartDate;
                while (nextDate <= _projectEndDate)
                {
                    nextDate = CalculateNextDate(nextDate, procedure.Frequency ?? 0, procedure.FrequencyType);

                    if (nextDate >= _projectStartDate && nextDate <= _projectEndDate)
                    {
                        if (!procedureDates.ContainsKey(nextDate) || 
                            ShouldKeepProcedure(nextDate, procedure, procedureDates[nextDate]))
                        {
                            procedureDates[nextDate] = procedure;
                        }
                    }
                }
            }

            foreach (var kvp in procedureDates)
            {
                var procedure = kvp.Value;
                var expenditureValue = (strategy.CostPerHour ?? 0) * (procedure.Duration ?? 0);
                var expenditure = new Expenditure
                {
                           
                    Location1 = asset.Location1,
                    Location2 = asset.Location2,
                    Location3 = asset.Location3,
                    Location4 = asset.Location4,
                    Asset_ID = asset.Asset_ID,
                    AssetDescription = asset.AssetDescription,
                    HierarchyL1 = asset.HierarchyL1,
                    HierarchyL2 = asset.HierarchyL2,
                    HierarchyL3 = asset.HierarchyL3,
                    HierarchyL4 = asset.HierarchyL4,
                    AssetHierarchy = asset.AssetHierarchy,
                    HierarchyCode = asset.HierarchyCode,
                    AcqDate = asset.AcqDate,
                    ExpenditureValue = expenditureValue,
                    ExpenditureDate = kvp.Key,
                    ExpenditureYear = kvp.Key.Year,
                    ExpenditureType = "Preventative Maintenance",
                    ExpenditureDescription = procedure.ProcedureDescription,
                    Comment = "",
                    };

                yield return expenditure;
            }
        }
        private DateTime CalculateNextDate(DateTime startDate, double frequency, string frequencyType)
        {
            var type = frequencyType.ToLowerInvariant();
            
            switch (type)
            {
                case "day":
                case "days":
                    return startDate.AddDays(frequency);
                    
                case "week":
                case "weeks":
                    return startDate.AddDays(frequency * 7);
                    
                case "month":
                case "months":
                    return startDate.AddMonths((int)frequency);
                    
                case "year":
                case "years":
                    return startDate.AddYears((int)frequency);
                    
                default:
                    throw new ArgumentException($"Unknown frequency type: {frequencyType}");
            }
        }

       private class FrequencyInfo
        {
            public double Frequency { get; set; }
            public string FrequencyType { get; set; }
            public int DaysCount { get; set; }
            public int Priority { get; set; }
        }

        private FrequencyInfo GetFrequencyInfo(double frequency, string frequencyType)
        {
            var type = frequencyType.ToLowerInvariant();
            var info = new FrequencyInfo
            {
                Frequency = frequency,
                FrequencyType = type,
                DaysCount = type switch
                {
                    "day" or "days" => (int)frequency,
                    "week" or "weeks" => (int)(frequency * 7),
                    "month" or "months" => (int)(frequency * 30),
                    "year" or "years" => (int)(frequency * 365),
                    _ => throw new ArgumentException($"Unknown frequency type: {frequencyType}")
                }
            };

            // Assign priority (higher number = higher priority)
            info.Priority = (type, frequency) switch
            {
                ("weeks", _) => 1,
                ("months", 1) => 2,
                ("months", 2) => 3,
                ("months", 3) => 4,
                ("months", 4) => 5,
                ("months", 6) => 6,
                ("months", 12) => 7,
                ("years", _) => 8,
                _ => 0
            };

            return info;
        }

        private bool ShouldKeepProcedure(DateTime date, MaintenanceProcedure newProcedure, MaintenanceProcedure existingProcedure)
        {
            var newInfo = GetFrequencyInfo(newProcedure.Frequency ?? 0, newProcedure.FrequencyType);
            var existingInfo = GetFrequencyInfo(existingProcedure.Frequency ?? 0, existingProcedure.FrequencyType);

            // If they're in the same month and one is yearly/monthly
            if (date.Year == existingProcedure.LastDoneDate?.Year && 
                date.Month == existingProcedure.LastDoneDate?.Month)
            {
                // If existing is yearly or higher frequency monthly (2,3,6,12), keep it
                if (existingInfo.FrequencyType == "years" || 
                    (existingInfo.FrequencyType == "months" && existingInfo.Frequency >= 2))
                {
                    return false;
                }
                
                // If new is yearly or higher frequency monthly (2,3,6,12), use it
                if (newInfo.FrequencyType == "years" || 
                    (newInfo.FrequencyType == "months" && newInfo.Frequency >= 2))
                {
                    return true;
                }
            }

            // For weekly vs monthly checks, look at ±3 days
            if ((newInfo.FrequencyType == "weeks" && existingInfo.FrequencyType == "months") ||
                (newInfo.FrequencyType == "months" && existingInfo.FrequencyType == "weeks"))
            {
                var daysDifference = Math.Abs((date - existingProcedure.LastDoneDate?.Date)?.Days ?? 0);
                if (daysDifference <= 3)
                {
                    // Keep the monthly, suppress the weekly
                    return newInfo.FrequencyType == "months";
                }
            }

            // If they have the same priority, keep the one with higher days count
            if (newInfo.Priority == existingInfo.Priority)
            {
                return newInfo.DaysCount > existingInfo.DaysCount;
            }

            // Otherwise, keep the one with higher priority
            return newInfo.Priority > existingInfo.Priority;
        }
        private IEnumerable<Expenditure> GenerateYearlyPMSummaries(IEnumerable<Expenditure> pmExpenditures)
        {
            var yearlyPMSummaries = pmExpenditures
                .Where(e => e.ExpenditureType?.Equals("Preventative Maintenance", StringComparison.OrdinalIgnoreCase) == true)
                .GroupBy(e => new 
                { 
                    e.Asset_ID, 
                    e.ExpenditureYear,
                    e.Location1,
                    e.Location2,
                    e.Location3,
                    e.Location4,
                    e.AssetDescription,
                    e.HierarchyL1,
                    e.HierarchyL2,
                    e.HierarchyL3,
                    e.HierarchyL4,
                    e.AssetHierarchy,
                    e.HierarchyCode,
                    e.AcqDate
                })
                .Select(g => new Expenditure
                {
                    Location1 = g.Key.Location1,
                    Location2 = g.Key.Location2,
                    Location3 = g.Key.Location3,
                    Location4 = g.Key.Location4,
                    Asset_ID = g.Key.Asset_ID,
                    AssetDescription = g.Key.AssetDescription,
                    HierarchyL1 = g.Key.HierarchyL1,
                    HierarchyL2 = g.Key.HierarchyL2,
                    HierarchyL3 = g.Key.HierarchyL3,
                    HierarchyL4 = g.Key.HierarchyL4,
                    AssetHierarchy = g.Key.AssetHierarchy,
                    HierarchyCode = g.Key.HierarchyCode,
                    ExpenditureValue = g.Sum(e => e.ExpenditureValue),
                    AcqDate = g.Key.AcqDate,
                    ExpenditureDate = new DateTime(g.Key.ExpenditureYear, 12, 31),
                    ExpenditureYear = g.Key.ExpenditureYear,
                    ExpenditureType = "Preventative Maintenance",
                    ExpenditureDescription = $"{g.Key.AssetDescription} - PM, Yearly Cost",
                    Comment = ""
                });

            var summariesList = yearlyPMSummaries.ToList();

            // If Flat mode is selected, generate CM records
            if (_isFlatModeSelected)
            {
                var cmRecords = summariesList.Select(pm => new Expenditure
                {
                    Location1 = pm.Location1,
                    Location2 = pm.Location2,
                    Location3 = pm.Location3,
                    Location4 = pm.Location4,
                    Asset_ID = pm.Asset_ID,
                    AssetDescription = pm.AssetDescription,
                    HierarchyL1 = pm.HierarchyL1,
                    HierarchyL2 = pm.HierarchyL2,
                    HierarchyL3 = pm.HierarchyL3,
                    HierarchyL4 = pm.HierarchyL4,
                    AssetHierarchy = pm.AssetHierarchy,
                    HierarchyCode = pm.HierarchyCode,
                    ExpenditureValue = pm.ExpenditureValue * 0.58, // 58% of PM cost
                    AcqDate = pm.AcqDate,
                    ExpenditureDate = pm.ExpenditureDate,
                    ExpenditureYear = pm.ExpenditureYear,
                    ExpenditureType = "Corrective Maintenance",
                    ExpenditureDescription = $"{pm.AssetDescription} - CM, Yearly Cost",
                    Comment = ""
                });

                // Return both PM and CM records
                return summariesList.Concat(cmRecords);
            }

            // If not in Flat mode, return only PM records
            return summariesList;
        }





        // Processes assets without Capital Projects
        private IEnumerable<Expenditure> ProcessAssetWithoutCapitalProjects(Asset asset, ClassData classData, int conditionRating)
        {
            DateTime initialReplacementDate;
            string initialComment = "";

            // Calculate initial replacement date based on condition or acquisition date
            if (conditionRating == 5)
            {
                initialReplacementDate = asset.PlannedStartDate ?? _projectStartDate;
                initialComment = "Based on Condition please replace the asset!";
            }
            else
            {
                if (asset.AcqDate.HasValue)
                {
                    initialReplacementDate = AddYearsFractional(asset.AcqDate.Value, classData.EstimatedLife.Value);
                    if (initialReplacementDate < (asset.PlannedStartDate ?? _projectStartDate))
                    {
                        double adjustedYears = classData.EstimatedLife.Value * 0.15;
                        initialReplacementDate = AddYearsFractional(asset.PlannedStartDate ?? _projectStartDate, adjustedYears);
                        initialComment = "Asset is approaching the end of its lifecycle, please monitor condition and expect to replace.";
                    }
                }
                else
                {
                    Console.WriteLine($"[Error] Asset {asset.Asset_ID} does not have an Acquisition Date. Cannot calculate Replacement Date.");
                    yield break;
                }
            }

            // Schedule all replacements first
            DateTime currentReplacementDate = initialReplacementDate;
            bool isFirstReplacement = true;

            while (currentReplacementDate <= _projectEndDate)
            {
                if (currentReplacementDate >= _projectStartDate)
                {
                    // Only use the initialComment for the first replacement
                    string currentComment = isFirstReplacement ? initialComment : "";
                    var replacementExpenditure = CreateExpenditure(asset, classData, currentReplacementDate, "Replacement", currentComment);
                    yield return replacementExpenditure;
                    Console.WriteLine($"[Scheduled] {replacementExpenditure.ExpenditureType} for Asset_ID {asset.Asset_ID} on {replacementExpenditure.ExpenditureDate.ToShortDateString()}.");
                }

                // Schedule refurbishments between replacements
                if (classData.RefurbishmentFrequency.HasValue && classData.RefurbishmentFrequency.Value > 0.0)
                {
                    DateTime nextReplacementDate = AddYearsFractional(currentReplacementDate, classData.EstimatedLife.Value);
                    DateTime refurbishmentDate = AddYearsFractional(currentReplacementDate, classData.RefurbishmentFrequency.Value);

                    while (refurbishmentDate < nextReplacementDate && refurbishmentDate <= _projectEndDate)
                    {
                        if (refurbishmentDate >= _projectStartDate)
                        {
                            var refurbishmentExpenditure = CreateExpenditure(asset, classData, refurbishmentDate, "Refurbishment", "");
                            yield return refurbishmentExpenditure;
                            Console.WriteLine($"[Scheduled] {refurbishmentExpenditure.ExpenditureType} for Asset_ID {asset.Asset_ID} on {refurbishmentExpenditure.ExpenditureDate.ToShortDateString()}.");
                        }
                        refurbishmentDate = AddYearsFractional(refurbishmentDate, classData.RefurbishmentFrequency.Value);
                    }
                }

                // Move to next replacement cycle
                currentReplacementDate = AddYearsFractional(currentReplacementDate, classData.EstimatedLife.Value);
                isFirstReplacement = false;
            }
        }
        // Processes assets with Capital Projects
        private IEnumerable<Expenditure> ProcessAssetWithCapitalProjects(Asset asset, ClassData classData, List<CapitalProject> assetCapitalProjects)
        {
            // Sort Capital Projects by ProjectStartYear
            var sortedCapitalProjects = assetCapitalProjects.OrderBy(cp => cp.ProjectStartYear).ToList();

            // To prevent duplicate Capital Projects on the same date, track scheduled CPs
            HashSet<DateTime> scheduledCPDates = new HashSet<DateTime>();

            foreach (var cp in sortedCapitalProjects)
            {
                // Handle multi-year Capital Projects by scheduling each year separately
                int startYear = cp.ProjectStartYear;
                int endYear = cp.ProjectEndYear != 0 ? cp.ProjectEndYear : cp.ProjectStartYear;
                int duration = endYear - startYear + 1;

                if (duration <= 0)
                {
                    Console.WriteLine($"[Error] Invalid duration for Capital Project '{cp.ProjectTitle}' (StartYear: {startYear}, EndYear: {endYear}). Skipping.");
                    continue;
                }

                double annualCost = duration > 1 ? cp.ProjectCost / duration : cp.ProjectCost;

                for (int year = startYear; year <= endYear; year++)
                {
                    DateTime cpDate;
                    try
                    {
                        cpDate = new DateTime(year, 7, 1); // Assuming July 1st
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Invalid date for Capital Project '{cp.ProjectTitle}' in year {year}: {ex.Message}. Skipping.");
                        continue;
                    }

                    if (scheduledCPDates.Contains(cpDate))
                    {
                        Console.WriteLine($"[Warning] Duplicate Capital Project date {cpDate.ToShortDateString()} for Asset_ID {asset.Asset_ID}. Skipping duplicate.");
                        continue;
                    }

                    scheduledCPDates.Add(cpDate);

                    // Schedule the Capital Project
                    var cpExpenditure = CreateCapitalProjectExpenditure(cp, cpDate, annualCost, asset);
                    yield return cpExpenditure;
                    Console.WriteLine($"[Scheduled] Capital Project for Asset_ID {asset.Asset_ID} on {cpDate.ToShortDateString()}.");

                    // Calculate Replacement Date based on Capital Project
                    DateTime replacementDate = AddYearsFractional(cpDate, classData.EstimatedLife.Value);

                    // Validate Replacement Date
                    if (replacementDate < _projectStartDate || replacementDate > _projectEndDate)
                    {
                        Console.WriteLine($"[Warning] Replacement Date {replacementDate.ToShortDateString()} for Asset_ID {asset.Asset_ID} after Capital Project on {cpDate.ToShortDateString()} is outside project duration. Skipping.");
                        continue;
                    }

                    // Create Replacement Expenditure
                    var replacementExpenditure = CreateExpenditure(asset, classData, replacementDate, "Replacement", "");
                    yield return replacementExpenditure;
                    Console.WriteLine($"[Scheduled] Replacement for Asset_ID {asset.Asset_ID} on {replacementExpenditure.ExpenditureDate.ToShortDateString()} after Capital Project on {cpDate.ToShortDateString()}.");

                    // Calculate Refurbishment Date before Replacement
                    double yearsBeforeReplacement = classData.EstimatedLife.Value - classData.RefurbishmentFrequency.Value;
                    DateTime refurbishmentDate = AddYearsFractional(replacementDate, -yearsBeforeReplacement);

                    // Ensure refurbishmentDate is after Capital Project Date and within project duration
                    if (refurbishmentDate >= cpDate && refurbishmentDate >= _projectStartDate && refurbishmentDate <= _projectEndDate)
                    {
                        var refurbishmentExpenditure = CreateExpenditure(asset, classData, refurbishmentDate, "Refurbishment", "");
                        yield return refurbishmentExpenditure;
                        Console.WriteLine($"[Scheduled] Refurbishment for Asset_ID {asset.Asset_ID} on {refurbishmentExpenditure.ExpenditureDate.ToShortDateString()} before Replacement.");
                    }
                    else
                    {
                        Console.WriteLine($"[Info] Skipping Refurbishment for Asset_ID {asset.Asset_ID} on {refurbishmentDate.ToShortDateString()} as it is before Capital Project Date or outside project duration.");
                    }

                    // Schedule future Refurbishments after Replacement Date based on Refurbishment Frequency
                    if (classData.RefurbishmentFrequency.HasValue && classData.RefurbishmentFrequency.Value > 0.0)
                    {
                        DateTime nextRefurbishmentDate = AddYearsFractional(replacementDate, classData.RefurbishmentFrequency.Value);

                        while (nextRefurbishmentDate <= _projectEndDate)
                        {
                            // Check if the next refurbishment date is after the next replacement date
                            // If so, schedule Replacement instead and reset refurbishment schedule
                            // (This logic needs to be implemented based on your specific requirements)

                            var nextRefurbishmentExpenditure = CreateExpenditure(asset, classData, nextRefurbishmentDate, "Refurbishment", "");
                            yield return nextRefurbishmentExpenditure;
                            Console.WriteLine($"[Scheduled] Refurbishment for Asset_ID {asset.Asset_ID} on {nextRefurbishmentExpenditure.ExpenditureDate.ToShortDateString()}.");
                            nextRefurbishmentDate = AddYearsFractional(nextRefurbishmentDate, classData.RefurbishmentFrequency.Value);
                        }
                    }
                }
            }}

            // Generates an Expenditure object for a Capital Project
            private Expenditure CreateCapitalProjectExpenditure(CapitalProject project, DateTime expenditureDate, double annualCost, Asset asset)
            {
                string assetDescription = asset?.AssetDescription ?? "No Description";

                Console.WriteLine($"[Creating] Capital Project for Asset_ID {project.Asset_ID ?? "N/A"} on {expenditureDate.ToShortDateString()} with value {annualCost}");

                return new Expenditure
                {
                    Location1 = project.Location1,
                    Location2 = project.Location2,
                    Location3 = project.Location3,
                    Location4 = project.Location4,
                    Asset_ID = project.Asset_ID,
                    AssetDescription = assetDescription,
                    HierarchyL1 = project.HierarchyL1,
                    HierarchyL2 = project.HierarchyL2,
                    HierarchyL3 = project.HierarchyL3,
                    HierarchyL4 = project.HierarchyL4,
                    AssetHierarchy = project.AssetHierarchy,
                    HierarchyCode = project.HierarchyCode,
                    ExpenditureValue = annualCost,
                    AcqDate = asset?.AcqDate,
                    ExpenditureDate = expenditureDate,
                    ExpenditureYear = expenditureDate.Year,
                    ExpenditureType = "Capital Project",
                    ExpenditureDescription = project.ProjectTitle,
                    Comment = "",
                };
            }

            // Generates expenditures for Capital Projects without Asset_ID
            private IEnumerable<Expenditure> GenerateCapitalProjectExpenditures(CapitalProject project)
            {
                // Handle multi-year Capital Projects by scheduling each year separately
                int startYear = project.ProjectStartYear;
                int endYear = project.ProjectEndYear != 0 ? project.ProjectEndYear : project.ProjectStartYear;
                int duration = endYear - startYear + 1;

                if (duration <= 0)
                {
                    Console.WriteLine($"[Error] Invalid duration for Capital Project '{project.ProjectTitle}' (StartYear: {startYear}, EndYear: {endYear}). Skipping.");
                    yield break;
                }

                double annualCost = duration > 1 ? project.ProjectCost / duration : project.ProjectCost;

                for (int year = startYear; year <= endYear; year++)
                {
                    DateTime cpDate;
                    try
                    {
                        cpDate = new DateTime(year, 7, 1); // Assuming July 1st
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Invalid date for Capital Project '{project.ProjectTitle}' in year {year}: {ex.Message}. Skipping.");
                        continue;
                    }

                    yield return new Expenditure
                    {
                        Location1 = project.Location1,
                        Location2 = project.Location2,
                        Location3 = project.Location3,
                        Location4 = project.Location4,
                        Asset_ID = project.Asset_ID, // This will be null or empty
                        AssetDescription = "No Asset", // Since there's no Asset_ID
                        HierarchyL1 = project.HierarchyL1,
                        HierarchyL2 = project.HierarchyL2,
                        HierarchyL3 = project.HierarchyL3,
                        HierarchyL4 = project.HierarchyL4,
                        AssetHierarchy = project.AssetHierarchy,
                        HierarchyCode = project.HierarchyCode,
                        ExpenditureValue = annualCost,
                        AcqDate = null, // No Asset_ID, so no Acquisition Date
                        ExpenditureDate = cpDate,
                        ExpenditureYear = year,
                        ExpenditureType = "Capital Project",
                        ExpenditureDescription = project.ProjectTitle,
                        Comment = "",
                    };
                }
            }

            // Helper method to create an Expenditure object
            private Expenditure CreateExpenditure(
                Asset asset, ClassData classData, DateTime date, string type, string comment)
            {
                double replacementCost = asset.PurchaseCost ?? classData.AvgReplacementCost ?? 0.0;
                double expenditureValue = type == "Replacement"
                    ? replacementCost
                    : replacementCost * (classData.RefurbishmentCostAsProportionOfReplacementCost ?? 0.0);

                Console.WriteLine($"[Creating] {type} for Asset_ID {asset.Asset_ID} on {date.ToShortDateString()} with value {expenditureValue}");

                return new Expenditure
                {
                    Location1 = asset.Location1,
                    Location2 = asset.Location2,
                    Location3 = asset.Location3,
                    Location4 = asset.Location4,
                    Asset_ID = asset.Asset_ID,
                    AssetDescription = asset.AssetDescription ?? "No Description",
                    HierarchyL1 = asset.HierarchyL1,
                    HierarchyL2 = asset.HierarchyL2,
                    HierarchyL3 = asset.HierarchyL3,
                    HierarchyL4 = asset.HierarchyL4,
                    AssetHierarchy = asset.AssetHierarchy,
                    HierarchyCode = asset.HierarchyCode,
                    ExpenditureValue = expenditureValue,
                    AcqDate = asset.AcqDate,
                    ExpenditureDate = date,
                    ExpenditureYear = date.Year,
                    ExpenditureType = type,
                    ExpenditureDescription = $"{type} - {asset.AssetDescription}",
                    Comment = comment
                };
            }

            // Method to add fractional years to a DateTime
            private static DateTime AddYearsFractional(DateTime date, double years)
            {
                try
                {
                    int wholeYears = (int)Math.Floor(years);
                    double fractionalYear = years - wholeYears;
                    int months = (int)Math.Round(fractionalYear * 12);
                    return date.AddYears(wholeYears).AddMonths(months);
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine($"[Error] Date overflow when adding {years} years to {date.ToShortDateString()}.");
                    return DateTime.MaxValue;
                }
            }
    } 
} 
