using System;
using System.IO;

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
    }
}
