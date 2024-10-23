using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls; // Add this line to resolve DatePicker errors
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using CoolMangoes.Modules;
using CoolMangoes.Models;  // For using Asset and ClassData classes

namespace CoolMangoes
{
    public partial class MainWindow : Window
    {
        private readonly DownloadService downloadService;
        private readonly AssetDataService assetDataService;
        private readonly ClassDataService classDataService;
        private readonly MaintenanceStrategiesService maintenanceStrategiesService;
        private readonly MaintenanceProceduresService maintenanceProceduresService;

        // Hold the uploaded asset and class data in memory after it's uploaded
        private List<Asset> uploadedAssetDataList;
        private List<ClassData> uploadedClassDataList;

        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        // private List<Expenditure> calculatedExpenditurePlan;
        private List<MaintenanceProcedure> uploadedMaintenanceProceduresList;


        public MainWindow()
        {
            InitializeComponent();
            downloadService = new DownloadService();
            assetDataService = new AssetDataService();
            classDataService = new ClassDataService();
            maintenanceStrategiesService = new MaintenanceStrategiesService();
            maintenanceProceduresService = new MaintenanceProceduresService();
        }

        private void ShowDataSection(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Data Section is active.");
        }

        private void ShowLCCSection(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("LCC Model Section is under development.");
        }

        private void ShowMaintenanceSection(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Maintenance Model Section is under development.");
        }

        // Downloads AssetData template directly to the Downloads folder
        private void DownloadAssetData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string saveFilePath = Path.Combine(downloadsFolder, "AssetDataTemplate.csv");

                downloadService.DownloadAssetDataTemplate();
                MessageBox.Show($"AssetData template has been downloaded to {saveFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the download: {ex.Message}");
            }
        }
        private void UploadAssetData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    uploadedAssetDataList = assetDataService.LoadAssetData(filePath);  // Store the uploaded data

                    if (uploadedAssetDataList == null || uploadedAssetDataList.Count == 0)
                    {
                        MessageBox.Show("Failed to load Asset Data.");
                    }
                    else
                    {
                        MessageBox.Show($"File processed successfully. Total records: {uploadedAssetDataList.Count}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during file upload: {ex.Message}");
                }
            }
        }


        // Downloads ClassData template based on processed AssetData and saves to Downloads folder
        private void DownloadClassData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if AssetData was uploaded first
                if (uploadedAssetDataList == null || uploadedAssetDataList.Count == 0)
                {
                    MessageBox.Show("Please upload AssetData first.");
                    return;
                }

                // Generate ClassDataTemplate
                var classDataList = ClassDataGenerator.GenerateClassDataTemplate(uploadedAssetDataList);

                // Save ClassDataTemplate to the Downloads folder
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string saveFilePath = Path.Combine(downloadsFolder, "ClassDataTemplate.csv");

                ClassDataGenerator.DownloadClassDataTemplate(classDataList, saveFilePath);
                MessageBox.Show($"ClassData template has been downloaded to {saveFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during download: {ex.Message}");
            }
        }

        // Downloads MaintenanceStrategies template directly to the Downloads folder
        private void DownloadMaintenanceStrategies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Assuming ClassData is already loaded previously in the app and is accessible
                var classDataList = classDataService.GetClassDataList(); // Get the loaded ClassData list
                
                if (classDataList == null || classDataList.Count == 0)
                {
                    MessageBox.Show("ClassData is empty or not loaded.");
                    return;
                }

                // Generate MaintenanceStrategies from ClassData where MaintenanceType = "Planned"
                var maintenanceStrategies = maintenanceStrategiesService.GenerateMaintenanceStrategies(classDataList);

                // Define the downloads folder
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string saveFilePath = Path.Combine(downloadsFolder, "MaintenanceStrategiesTemplate.csv");

                // Download the MaintenanceStrategies template
                maintenanceStrategiesService.DownloadMaintenanceStrategiesTemplate(maintenanceStrategies, saveFilePath);

                MessageBox.Show($"MaintenanceStrategies template has been downloaded to {saveFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the download: {ex.Message}");
            }
        }

        // Downloads MaintenanceProcedures template directly to the Downloads folder
        private void DownloadMaintenanceProcedures_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uploadedAssetDataList == null || uploadedClassDataList == null)
                {
                    MessageBox.Show("Please upload both AssetData and Maintenance Strategies first.");
                    return;
                }

                // Generate MaintenanceProcedures based on uploaded data
                var strategies = maintenanceStrategiesService.GetMaintenanceStrategiesList();  // Now this works
                var procedures = maintenanceProceduresService.GenerateMaintenanceProcedures(strategies, uploadedAssetDataList);

                // Save the procedures to the Downloads folder
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string saveFilePath = Path.Combine(downloadsFolder, "MaintenanceProceduresTemplate.csv");

                maintenanceProceduresService.DownloadMaintenanceProceduresTemplate(procedures, saveFilePath);
                MessageBox.Show($"MaintenanceProcedures template has been downloaded to {saveFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the download: {ex.Message}");
            }
        }

        // Uploads AssetData from a CSV file
        // private void UploadAssetData_Click(object sender, RoutedEventArgs e)
        // {
        //     OpenFileDialog openFileDialog = new OpenFileDialog
        //     {
        //         Filter = "CSV files (*.csv)|*.csv"
        //     };

        //     if (openFileDialog.ShowDialog() == true)
        //     {
        //         try
        //         {
        //             string filePath = openFileDialog.FileName;
        //             uploadedAssetDataList = assetDataService.LoadAssetData(filePath);  // Store the uploaded data
        //             MessageBox.Show($"File processed successfully. Total Asset records: {uploadedAssetDataList.Count}");
        //         }
        //         catch (Exception ex)
        //         {
        //             MessageBox.Show($"An error occurred during file upload: {ex.Message}");
        //         }
        //     }
        // }

        // Uploads ClassData from a CSV file
        private void UploadClassData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;

                    // Load ClassData using ClassDataService
                    uploadedClassDataList = classDataService.LoadClassData(filePath);

                    // Show the total number of records processed
                    MessageBox.Show($"File processed successfully. Total Class records: {uploadedClassDataList.Count}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during file upload: {ex.Message}");
                }
            }
        }

        // Uploads MaintenanceStrategies from a CSV file
        private void UploadMaintenanceStrategies_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    var strategiesList = maintenanceStrategiesService.LoadMaintenanceStrategies(filePath);
                    MessageBox.Show($"File processed successfully. Total Maintenance Strategies: {strategiesList.Count}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during file upload: {ex.Message}");
                }
            }
        }

        // Handler for UploadMaintenanceProcedures_Click (this is likely where you'll add functionality later)
        private void UploadMaintenanceProcedures_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    // Load Maintenance Procedures using MaintenanceProceduresService
                    uploadedMaintenanceProceduresList = maintenanceProceduresService.LoadMaintenanceProcedures(filePath);  // Assign to the correct variable

                    if (uploadedMaintenanceProceduresList == null || uploadedMaintenanceProceduresList.Count == 0)
                    {
                        MessageBox.Show("Failed to load Maintenance Procedures data or the list is empty.");
                    }
                    else
                    {
                        MessageBox.Show($"Maintenance Procedures data loaded successfully. Total records: {uploadedMaintenanceProceduresList.Count}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during Maintenance Procedures file upload: {ex.Message}");
                }
            }
        }


        // Handler for Flat (58% PM) corrective method
        private void FlatMethodButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Flat (58% PM) method selected.");
            // You can add logic for handling this method here
        }

        // Handler for Condition Based corrective method
        private void ConditionBasedMethodButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Condition Based method selected.");
            // You can add logic for handling this method here
        }

        // Project Start Date Calendar Selection
        private void ProjectStartDate_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker picker && picker.SelectedDate.HasValue)
            {
                ProjectStartDate = picker.SelectedDate.Value;
                MessageBox.Show($"Project Start Date: {ProjectStartDate.Value.ToShortDateString()}");
            }
        }

        // Project End Date Calendar Selection
        // Project End Date Calendar Selection
        private void ProjectEndDate_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker picker && picker.SelectedDate.HasValue)
            {
                ProjectEndDate = picker.SelectedDate.Value;
                MessageBox.Show($"Project End Date: {ProjectEndDate.Value.ToShortDateString()}");
            }
        }


        // Handler for LCC Model Calculate Button
        
        private void CalculateLCCModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if Asset data is loaded
                if (uploadedAssetDataList == null)
                {
                    MessageBox.Show("Asset data list is null.");
                    return;
                }
                if (uploadedAssetDataList.Count == 0)
                {
                    MessageBox.Show("Asset data list is empty.");
                    return;
                }

                // Check if Class data is loaded
                if (uploadedClassDataList == null)
                {
                    MessageBox.Show("Class data list is null.");
                    return;
                }
                if (uploadedClassDataList.Count == 0)
                {
                    MessageBox.Show("Class data list is empty.");
                    return;
                }

                // Check if Maintenance Procedures data is loaded
                if (uploadedMaintenanceProceduresList == null)
                {
                    MessageBox.Show("Maintenance Procedures data list is null.");
                    return;
                }
                if (uploadedMaintenanceProceduresList.Count == 0)
                {
                    MessageBox.Show("Maintenance Procedures data list is empty.");
                    return;
                }

                // Ensure ProjectStartDate and ProjectEndDate are selected
                if (ProjectStartDate == null || ProjectEndDate == null)
                {
                    MessageBox.Show("Please select a Project Start Date and Project End Date.");
                    return;
                }

                // Pass the selected project start and end dates into the service
                var expenditurePlanService = new ExpenditurePlanService(
                    uploadedAssetDataList, 
                    uploadedClassDataList, 
                    uploadedMaintenanceProceduresList, 
                    ProjectStartDate.Value, 
                    ProjectEndDate.Value);

                var expenditurePlans = expenditurePlanService.GenerateExpenditurePlan();

                // Handle the calculated expenditure plans (store, display, etc.)
                MessageBox.Show("Calculation completed successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }



        // Handler for LCC Model Download Button
        private void DownloadLCCModel_Click(object sender, RoutedEventArgs e)
        {
            
        }


        // Handler for AMP Model Calculate Button
        private void CalculateAMPModel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Calculating AMP Model...");
            // Add your AMP Model calculation logic here
        }

        // Handler for AMP Model Download Button
        private void DownloadAMPModel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Downloading AMP Model...");
            // Add your AMP Model download logic here
        }


        // Handler for CalculateButton_Click (this is likely where you'll add functionality later)
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Calculation functionality is under development.");
        }

        // Handler for DownloadCostModelButton_Click (this is likely where you'll add functionality later)
        private void DownloadCostModelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cost Model download functionality is under development.");
        }

        private void FlatButton_Click(object sender, RoutedEventArgs e)
        {
            FlatButton.Style = (Style)FindResource("SelectedCorrectiveButtonStyle");
            ConditionButton.Style = (Style)FindResource("ActionButtonStyle");
        }

        private void ConditionButton_Click(object sender, RoutedEventArgs e)
        {
            ConditionButton.Style = (Style)FindResource("SelectedCorrectiveButtonStyle");
            FlatButton.Style = (Style)FindResource("ActionButtonStyle");
        }
    }
    
}
