using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoolMangoes.Modules;
using CoolMangoes.Models;
using System.Windows.Threading;
using System.Windows.Controls;

namespace CoolMangoes
{
    public partial class MainWindow : Window
    {
        private readonly DownloadService downloadService;
        private readonly AssetDataService assetDataService;
        private readonly ClassDataService classDataService;
        private readonly MaintenanceStrategiesService maintenanceStrategiesService;
        private readonly MaintenanceProceduresService maintenanceProceduresService;

        private List<Asset> uploadedAssetDataList;
        private List<ClassData> uploadedClassDataList;
        private List<MaintenanceProcedure> uploadedMaintenanceProceduresList;
        private List<MaintenanceStrategy> uploadedMaintenanceStrategiesList;
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        private List<CapitalProject> uploadedCapitalProjectsList;
        private readonly CapitalProjectService capitalProjectService;

        private bool _isFlatModeSelected = false;
        

        public MainWindow()
        {
            InitializeComponent();
            downloadService = new DownloadService();
            assetDataService = new AssetDataService();
            classDataService = new ClassDataService();
            maintenanceStrategiesService = new MaintenanceStrategiesService();
            maintenanceProceduresService = new MaintenanceProceduresService();
            capitalProjectService = new CapitalProjectService();
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
                downloadService.DownloadAssetDataTemplate();
                MessageBox.Show("AssetData template has been downloaded to your Downloads folder.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the download: {ex.Message}");
            }
        }

        // Uploads AssetData from a CSV file
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
                    uploadedAssetDataList = assetDataService.LoadAssetData(filePath);

                    if (uploadedAssetDataList == null || uploadedAssetDataList.Count == 0)
                    {
                        MessageBox.Show("Failed to load Asset Data.");
                    }
                    else
                    {
                        MessageBox.Show($"Asset Data loaded successfully. Total records: {uploadedAssetDataList.Count}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during Asset Data upload: {ex.Message}");
                }
            }
        }

        // Downloads ClassData template based on processed AssetData and saves to Downloads folder
        private void DownloadClassData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uploadedAssetDataList == null || uploadedAssetDataList.Count == 0)
                {
                    MessageBox.Show("Please upload AssetData first.");
                    return;
                }

                // Generate ClassDataTemplate
                var classDataGenerator = new ClassDataGenerator();
                var classDataList = classDataGenerator.GenerateClassDataTemplate(uploadedAssetDataList);

                // Save ClassDataTemplate to the Downloads folder
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string saveFilePath = Path.Combine(downloadsFolder, "ClassDataTemplate.csv");

                classDataGenerator.DownloadClassDataTemplate(classDataList, saveFilePath);
                MessageBox.Show($"ClassData template has been downloaded to {saveFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during download: {ex.Message}");
            }
        }


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
                    uploadedClassDataList = classDataService.LoadClassData(filePath);

                    if (uploadedClassDataList == null || uploadedClassDataList.Count == 0)
                    {
                        MessageBox.Show("Failed to load Class Data.");
                    }
                    else
                    {
                        MessageBox.Show($"Class Data loaded successfully. Total records: {uploadedClassDataList.Count}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during Class Data upload: {ex.Message}");
                }
            }
        }

        // Downloads MaintenanceStrategies template directly to the Downloads folder
        private void DownloadMaintenanceStrategies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uploadedClassDataList == null || uploadedClassDataList.Count == 0)
                {
                    MessageBox.Show("Please upload Class Data first.");
                    return;
                }

                var maintenanceStrategies = maintenanceStrategiesService.GenerateMaintenanceStrategies(uploadedClassDataList);

                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string saveFilePath = Path.Combine(downloadsFolder, "MaintenanceStrategiesTemplate.csv");

                maintenanceStrategiesService.DownloadMaintenanceStrategiesTemplate(maintenanceStrategies, saveFilePath);
                MessageBox.Show($"MaintenanceStrategies template has been downloaded to {saveFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during Maintenance Strategies download: {ex.Message}");
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
                try
                {
                    string filePath = openFileDialog.FileName;
                    uploadedMaintenanceStrategiesList = maintenanceStrategiesService.LoadMaintenanceStrategies(filePath);

                    if (uploadedMaintenanceStrategiesList == null || uploadedMaintenanceStrategiesList.Count == 0)
                    {
                        MessageBox.Show("Failed to load Maintenance Strategies.");
                    }
                    else
                    {
                        MessageBox.Show($"Maintenance Strategies loaded successfully. Total records: {uploadedMaintenanceStrategiesList.Count}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during Maintenance Strategies upload: {ex.Message}");
                }
            }
        }

        // Downloads MaintenanceProcedures template directly to the Downloads folder
        private void DownloadMaintenanceProcedures_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uploadedAssetDataList == null || uploadedMaintenanceStrategiesList == null)
                {
                    MessageBox.Show("Please upload both Asset Data and Maintenance Strategies first.");
                    return;
                }

                var procedures = maintenanceProceduresService.GenerateMaintenanceProcedures(uploadedMaintenanceStrategiesList, uploadedAssetDataList);

                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string saveFilePath = Path.Combine(downloadsFolder, "MaintenanceProceduresTemplate.csv");

                maintenanceProceduresService.DownloadMaintenanceProceduresTemplate(procedures, saveFilePath);
                MessageBox.Show($"MaintenanceProcedures template has been downloaded to {saveFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during Maintenance Procedures download: {ex.Message}");
            }
        }

        // Uploads MaintenanceProcedures from a CSV file
        private void UploadMaintenanceProcedures_Click(object sender, RoutedEventArgs e)
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
                    uploadedMaintenanceProceduresList = maintenanceProceduresService.LoadMaintenanceProcedures(filePath);

                    if (uploadedMaintenanceProceduresList == null || uploadedMaintenanceProceduresList.Count == 0)
                    {
                        MessageBox.Show("Failed to load Maintenance Procedures.");
                    }
                    else
                    {
                        MessageBox.Show($"Maintenance Procedures loaded successfully. Total records: {uploadedMaintenanceProceduresList.Count}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during Maintenance Procedures upload: {ex.Message}");
                }
            }
        }

        // Project Start Date Calendar Selection
        private void ProjectStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker picker && picker.SelectedDate.HasValue)
            {
                ProjectStartDate = picker.SelectedDate.Value;
            }
        }

        // Project End Date Calendar Selection
        private void ProjectEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker picker && picker.SelectedDate.HasValue)
            {
                ProjectEndDate = picker.SelectedDate.Value;
            }
        }

        // Handler for LCC Model Calculate Button
        private async void CalculateLCCModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uploadedAssetDataList == null || uploadedClassDataList == null || uploadedMaintenanceProceduresList == null || uploadedMaintenanceStrategiesList == null)
                {
                    MessageBox.Show("Please upload all required data (Asset Data, Class Data, Maintenance Strategies, and Procedures) before calculating.");
                    return;
                }

                if (ProjectStartDate == null || ProjectEndDate == null)
                {
                    MessageBox.Show("Please select both Project Start Date and Project End Date.");
                    return;
                }

                ProgressBar.Value = 0;
                StatusText.Text = "Starting calculation...";

                var progress = new Progress<int>(percent =>
                {
                    ProgressBar.Value = percent;
                    StatusText.Text = $"Progress: {percent}%";
                });

                await Task.Run(() =>
                {
                    var expenditurePlanService = new ExpenditurePlanService(
                        uploadedAssetDataList,
                        uploadedClassDataList,
                        uploadedMaintenanceProceduresList,
                        uploadedMaintenanceStrategiesList, // Pass the maintenance strategies list here
                        ProjectStartDate.Value,
                        ProjectEndDate.Value,
                        uploadedCapitalProjectsList
                    );

                    var expenditurePlans = expenditurePlanService.GenerateExpenditurePlan();

                    string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    string filePath = Path.Combine(downloadsFolder, "LCC_CostModel.xlsx");

                    downloadService.DownloadLCCCostModel(expenditurePlans, filePath, progress);
                });

                MessageBox.Show("Calculation completed!");
                StatusText.Text = "Calculation completed!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during LCC Model calculation: {ex.Message}");
                StatusText.Text = "Error occurred!";
            }
        }

        // Handler for LCC Model Download Button
        private async void DownloadLCCModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uploadedAssetDataList == null || uploadedAssetDataList.Count == 0)
                {
                    MessageBox.Show("No data available for download.");
                    return;
                }

                if (ProjectStartDate == null || ProjectEndDate == null)
                {
                    MessageBox.Show("Please select both Project Start Date and Project End Date.");
                    return;
                }

                ProgressBar.Value = 0;
                StatusText.Text = "Starting download...";

                var progress = new Progress<int>(percent =>
                {
                    ProgressBar.Value = percent;
                    StatusText.Text = $"Progress: {percent}%";
                });

                await Task.Run(() =>
                {
                    var expenditurePlanService = new ExpenditurePlanService(
                        uploadedAssetDataList, 
                        uploadedClassDataList, 
                        uploadedMaintenanceProceduresList, 
                        uploadedMaintenanceStrategiesList,
                        ProjectStartDate.Value, 
                        ProjectEndDate.Value,
                        uploadedCapitalProjectsList,
                        _isFlatModeSelected
                    );

                    var expenditureData = expenditurePlanService.GenerateExpenditurePlan();

                    string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    string filePath = Path.Combine(downloadsFolder, "LCC_CostModel.xlsx");

                    downloadService.DownloadLCCCostModel(expenditureData, filePath, progress);
                });

                MessageBox.Show($"File has been downloaded successfully!");
                StatusText.Text = "Download completed!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while downloading: {ex.Message}");
                StatusText.Text = "Error occurred!";
            }
        }

 
        private async void PMSchedule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (uploadedAssetDataList == null || uploadedClassDataList == null || 
                    uploadedMaintenanceProceduresList == null || uploadedMaintenanceStrategiesList == null)
                {
                    MessageBox.Show("Please upload all required data before generating PM Schedule.");
                    return;
                }

                if (ProjectStartDate == null || ProjectEndDate == null)
                {
                    MessageBox.Show("Please select both Project Start Date and Project End Date.");
                    return;
                }

                ProgressBar.Value = 0;
                StatusText.Text = "Generating PM Schedule...";

                var progress = new Progress<int>(percent =>
                {
                    ProgressBar.Value = percent;
                    StatusText.Text = $"Progress: {percent}%";
                });

                await Task.Run(() =>
                {
                    var expenditurePlanService = new ExpenditurePlanService(
                        uploadedAssetDataList,
                        uploadedClassDataList,
                        uploadedMaintenanceProceduresList,
                        uploadedMaintenanceStrategiesList,
                        ProjectStartDate.Value,
                        ProjectEndDate.Value,
                        uploadedCapitalProjectsList
                    );

                    // Use the new method specifically for PM Schedule
                    var pmScheduleData = expenditurePlanService.GeneratePMSchedule().ToList();

                    if (!pmScheduleData.Any())
                    {
                        Dispatcher.Invoke(() => MessageBox.Show("No preventative maintenance data found."));
                        return;
                    }

                    string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    string filePath = Path.Combine(downloadsFolder, "PM_Schedule.xlsx");

                    downloadService.DownloadPreventativeMaintenance(pmScheduleData, filePath, progress);
                });

                MessageBox.Show("PM Schedule generated successfully!");
                StatusText.Text = "PM Schedule completed!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating PM Schedule: {ex.Message}");
                StatusText.Text = "Error occurred!";
            }
        }

        // Handler for AMP Model Download Button
        private void DownloadAMPModel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Downloading AMP Model...");
            // Add AMP Model download logic here
        }

        // Handler for Corrective actions (Under development)
        private void DownladCapital_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                downloadService.DownloadCapitalProjectsTemplate();
                MessageBox.Show("Capital Projects template has been downloaded to your Downloads folder.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the download: {ex.Message}");
            }
        }

        private void UploadCapital_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                capitalProjectService.UploadCapitalProjectsData();

                uploadedCapitalProjectsList = capitalProjectService.UploadedCapitalProjects;

                if (uploadedCapitalProjectsList == null || uploadedCapitalProjectsList.Count == 0)
                {
                    MessageBox.Show("Failed to load Capital Projects data.");
                }
                else
                {
                    MessageBox.Show($"Capital Projects data loaded successfully. Total records: {uploadedCapitalProjectsList.Count}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during Capital Projects data upload: {ex.Message}");
            }
        }
        // Handler for Flat (58% PM) corrective method
       private void FlatButton_Click(object sender, RoutedEventArgs e)
        {
            FlatButton.Style = (Style)FindResource("SelectedResetButtonStyle");
            ConditionButton.Style = (Style)FindResource("ActionButtonStyle");
            
            // Disable and reset Reset at Refurbishment buttons
            YesButton.IsEnabled = false;
            NoButton.IsEnabled = false;
            YesButton.Style = (Style)FindResource("ActionButtonStyle");
            NoButton.Style = (Style)FindResource("ActionButtonStyle");

            // Store the selected mode
            _isFlatModeSelected = true;
        }

        private void ConditionButton_Click(object sender, RoutedEventArgs e)
        {
            ConditionButton.Style = (Style)FindResource("SelectedResetButtonStyle");
            FlatButton.Style = (Style)FindResource("ActionButtonStyle");
            
            // Enable Reset at Refurbishment buttons
            YesButton.IsEnabled = true;
            NoButton.IsEnabled = true;

            // Store the selected mode
            _isFlatModeSelected = false;
        }

        // Handler for Adjust Corrective method
        private void AdjustCorrective_Click(object sender, RoutedEventArgs e)
        {
            if (!YesButton.IsEnabled) return; // Extra protection against clicks when disabled
            
            YesButton.Style = (Style)FindResource("SelectedResetButtonStyle");
            NoButton.Style = (Style)FindResource("ActionButtonStyle");
        }

        // Handler for Leave Corrective method
        private void LeaveCorrective_Click(object sender, RoutedEventArgs e)
        {
            if (!NoButton.IsEnabled) return; // Extra protection against clicks when disabled
            
            NoButton.Style = (Style)FindResource("SelectedResetButtonStyle");
            YesButton.Style = (Style)FindResource("ActionButtonStyle");
        }
    }
}
