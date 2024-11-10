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
using System.Text;
using ClosedXML.Excel;

namespace CoolMangoes
{
    public partial class MainWindow : Window
    {
        private readonly DownloadService downloadService;
        private readonly AssetDataService assetDataService;
        private readonly ClassDataService classDataService;
        private readonly MaintenanceStrategiesService maintenanceStrategiesService;
        private readonly MaintenanceProceduresService maintenanceProceduresService;
        private readonly WorkbookSheetService workbookSheetService;

        private List<Asset> uploadedAssetDataList;
        private List<ClassData> uploadedClassDataList;
        private List<MaintenanceProcedure> uploadedMaintenanceProceduresList;
        private List<MaintenanceStrategy> uploadedMaintenanceStrategiesList;
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        private List<CapitalProject> uploadedCapitalProjectsList;
        private readonly CapitalProjectService capitalProjectService;

        private bool _isFlatModeSelected = false;
        private bool _isLeaveCorrectiveSelected = false;
        private bool _isAdjustCorrectiveSelected = false;

        private List<HierarchyItem> hierarchyData;
        private List<Expenditure> expenditures;
        

        public MainWindow()
        {
            InitializeComponent();
            workbookSheetService = new WorkbookSheetService();
            downloadService = new DownloadService(workbookSheetService); 
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
                if (!ValidateInputs())
                    return;

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
                        uploadedMaintenanceStrategiesList,
                        ProjectStartDate.Value,
                        ProjectEndDate.Value,
                        uploadedCapitalProjectsList,
                        _isFlatModeSelected,
                        _isLeaveCorrectiveSelected,
                        _isAdjustCorrectiveSelected
                    );

                    // Store the expenditures in the class field for later use
                    expenditures = expenditurePlanService.GenerateExpenditurePlan().ToList();

                    string downloadsFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                        "Downloads");
                    string filePath = Path.Combine(downloadsFolder, "LCC_CostModel.xlsx");

                    downloadService.DownloadLCCCostModel(
                        expenditures,
                        uploadedAssetDataList,
                        uploadedClassDataList,
                        uploadedMaintenanceStrategiesList,
                        uploadedMaintenanceProceduresList,
                        ProjectStartDate.Value,
                        ProjectEndDate.Value,
                        filePath,
                        progress
                    );
                });

                MessageBox.Show("Calculation completed and LCC Cost Model has been saved!");
                StatusText.Text = "Calculation completed!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                StatusText.Text = "Error occurred!";
            }
        }

        // Handler for LCC Model Download Button
        private async void DownloadLCCModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

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
                        _isFlatModeSelected,
                        _isLeaveCorrectiveSelected,
                        _isAdjustCorrectiveSelected
                    );

                    var expenditureData = expenditurePlanService.GenerateExpenditurePlan();

                    string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    string filePath = Path.Combine(downloadsFolder, "LCC_CostModel.xlsx");

                    downloadService.DownloadLCCCostModel(
                        expenditureData,
                        uploadedAssetDataList,
                        uploadedClassDataList,
                        uploadedMaintenanceStrategiesList,
                        uploadedMaintenanceProceduresList,
                        ProjectStartDate.Value,
                        ProjectEndDate.Value,
                        filePath,
                        progress
                    );
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
            try
            {
                if (!ValidateInputs())
                    return;

                // Check if expenditures have been calculated
                if (expenditures == null || !expenditures.Any())
                {
                    MessageBox.Show("Please calculate the LCC Model first to generate expenditures.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get hierarchy data - either from uploaded hierarchy or build from asset data
                var hierarchyData = GetHierarchyData();
                
                // Show preview window first with a callback for AMP download
                ShowHierarchyPreviewWithDownload(hierarchyData, () => 
                {
                    try
                    {
                        var saveFileDialog = new SaveFileDialog
                        {
                            Filter = "Excel files (*.xlsx)|*.xlsx",
                            FileName = "AMPModel.xlsx"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            var progress = new Progress<int>(value =>
                            {
                                // Update progress if needed
                                ProgressBar.Value = value;
                                StatusText.Text = $"Generating AMP Model: {value}%";
                            });
                            
                            downloadService.DownloadAMPModel(
                                uploadedAssetDataList,
                                uploadedClassDataList,
                                hierarchyData,
                                expenditures,
                                uploadedMaintenanceStrategiesList,
                                uploadedMaintenanceProceduresList, 
                                ProjectStartDate.Value,
                                ProjectEndDate.Value,
                                saveFileDialog.FileName,
                                progress
                            );

                            MessageBox.Show(
                                "AMP Model has been successfully saved!",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error saving AMP Model: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparing AMP Model: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (uploadedAssetDataList == null || !uploadedAssetDataList.Any())
            {
                MessageBox.Show("Please upload Asset Data first.");
                return false;
            }

            if (ProjectStartDate == null || ProjectEndDate == null)
            {
                MessageBox.Show("Please select both Project Start Date and Project End Date.");
                return false;
            }

            return true;
        }

        private List<HierarchyItem> GetHierarchyData()
        {
            // Check if hierarchy template was already uploaded
            if (hierarchyData != null && hierarchyData.Any())
            {
                return hierarchyData;
            }

            // Build location hierarchy from asset data
            return BuildLocationHierarchy(uploadedAssetDataList);
        }

        private void GenerateHierarchyLine(HierarchyItem item, string indent, bool isLast, StringBuilder preview)
        {
            string treeChar = isLast ? "└─" : "├─";
            string description = item.Description ?? "";
            string id = item.IsAssetOrParent ? item.ID : "";  // Only show ID for Asset_ID and Parent_ID
            
            // Combine tree visualization, description, and ID
            preview.AppendLine($"{indent}{treeChar}{description,-70}{id}");
            
            for (int i = 0; i < item.Children.Count; i++)
            {
                string newIndent = indent + (isLast ? "  " : "│ ");
                GenerateHierarchyLine(item.Children[i], newIndent, i == item.Children.Count - 1, preview);
            }
        }

        private void ShowHierarchyPreview(List<HierarchyItem> hierarchyData)
        {
            var previewWindow = new Window
            {
                Title = "Hierarchy Preview",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            // Create main container
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create worksheet for preview
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Hierarchy Preview");
                
                // Add headers
                worksheet.Cell(1, 1).Value = "Description";
                worksheet.Cell(1, 2).Value = "ID";
                
                // Format headers
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Add hierarchy data
                int row = 2;
                foreach (var item in hierarchyData)
                {
                    AddHierarchyToExcel(worksheet, item, "", true, ref row);
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();
                
                // Apply grouping
                var lastRow = worksheet.LastRowUsed().RowNumber();
                var levelRows = new Dictionary<int, List<int>>();
                
                // Collect rows for each level
                for (int i = 2; i <= lastRow; i++)
                {
                    string id = worksheet.Cell(i, 2).GetString();
                    int level = GetHierarchyLevel(id);
                    
                    if (!levelRows.ContainsKey(level))
                        levelRows[level] = new List<int>();
                    levelRows[level].Add(i);
                }

                // Apply grouping for each level
                foreach (var level in levelRows.Keys.OrderByDescending(k => k))
                {
                    foreach (var rowNum in levelRows[level])
                    {
                        int groupEnd = FindGroupEnd(worksheet, rowNum, lastRow, level);
                        if (groupEnd > rowNum)
                        {
                            worksheet.Rows(rowNum, groupEnd).Group(level);
                            worksheet.Row(rowNum).Collapse();
                        }
                    }
                }

                // Convert worksheet to string for preview
                var preview = new StringBuilder();
                for (int i = 1; i <= lastRow; i++)
                {
                    var description = worksheet.Cell(i, 1).GetString();
                    var id = worksheet.Cell(i, 2).GetString();
                    preview.AppendLine($"{description,-70}{id}");
                }

                // Add preview text box
                var textBox = new TextBox
                {
                    Text = preview.ToString(),
                    IsReadOnly = true,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    Margin = new Thickness(5)
                };
                Grid.SetRow(textBox, 0);
                grid.Children.Add(textBox);

                // Add button panel
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(5)
                };
                Grid.SetRow(buttonPanel, 1);

                // Add download button
                var downloadButton = new Button
                {
                    Content = "Download Hierarchy",
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(5)
                };
                downloadButton.Click += (s, e) =>
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel files (*.xlsx)|*.xlsx",
                        FileName = "BuiltHierarchy.xlsx"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        try
                        {
                            downloadService.SaveHierarchyToFile(hierarchyData, saveFileDialog.FileName);
                            MessageBox.Show(
                                "Hierarchy has been successfully saved!",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Error saving hierarchy: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                };
                buttonPanel.Children.Add(downloadButton);

                // Add close button
                var closeButton = new Button
                {
                    Content = "Close",
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(5)
                };
                closeButton.Click += (s, e) => previewWindow.Close();
                buttonPanel.Children.Add(closeButton);

                grid.Children.Add(buttonPanel);
                previewWindow.Content = grid;
                previewWindow.ShowDialog();
            }
        }
        private void AddHierarchyToExcel(IXLWorksheet worksheet, HierarchyItem item, string indent, bool isLast, ref int row)
        {
            string treeChar = isLast ? "└─" : "├─";
            string description = item.Description ?? "";
            
            // Combine tree visualization and description
            worksheet.Cell(row, 1).Value = indent + treeChar + description;
            worksheet.Cell(row, 2).Value = item.IsAssetOrParent ? item.ID : "";  // Only show ID for Asset_ID and Parent_ID
            
            row++;
            
            for (int i = 0; i < item.Children.Count; i++)
            {
                string newIndent = indent + (isLast ? "  " : "│ ");
                AddHierarchyToExcel(worksheet, item.Children[i], newIndent, i == item.Children.Count - 1, ref row);
            }
        }

        private int FindGroupEnd(IXLWorksheet worksheet, int startRow, int lastRow, int parentLevel)
        {
            for (int i = startRow + 1; i <= lastRow; i++)
            {
                string id = worksheet.Cell(i, 2).GetString();
                int currentLevel = GetHierarchyLevel(id);
                if (currentLevel <= parentLevel)
                {
                    return i - 1;
                }
            }
            return lastRow;
        }

        private int GetHierarchyLevel(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return id.Count(c => c == '-');
        }

        private List<HierarchyItem> BuildLocationHierarchy(List<Asset> assets)
        {
            if (assets == null || !assets.Any())
            {
                throw new Exception("No asset data available to build hierarchy.");
            }

            var hierarchyItems = new List<HierarchyItem>();
            var processedNodes = new HashSet<string>();  // Track unique nodes by description + level
            var assetIdLookup = assets.ToDictionary(a => a.Asset_ID, a => a);

            // Validate Asset_IDs and Parent_IDs
            ValidateAssetData(assets);

            foreach (var asset in assets)
            {
                string currentParent = "";
                int nextLevel = 1;

                // Process Location1
                if (!string.IsNullOrEmpty(asset.Location1))
                {
                    var nodeKey = $"L1_{asset.Location1}";
                    if (!processedNodes.Contains(nodeKey))
                    {
                        hierarchyItems.Add(new HierarchyItem
                        {
                            ID = "",  // Locations don't have IDs
                            Description = asset.Location1,
                            Parent_ID = "",
                            Level = nextLevel,
                            Children = new List<HierarchyItem>()
                        });
                        processedNodes.Add(nodeKey);
                    }
                    currentParent = asset.Location1;
                    nextLevel++;
                }

                // Process Location2
                if (!string.IsNullOrEmpty(asset.Location2))
                {
                    var nodeKey = $"L2_{asset.Location2}";
                    if (!processedNodes.Contains(nodeKey))
                    {
                        hierarchyItems.Add(new HierarchyItem
                        {
                            ID = "",
                            Description = asset.Location2,
                            Parent_ID = currentParent,
                            Level = nextLevel,
                            Children = new List<HierarchyItem>()
                        });
                        processedNodes.Add(nodeKey);
                    }
                    currentParent = asset.Location2;
                    nextLevel++;
                }

                // Process Location3
                if (!string.IsNullOrEmpty(asset.Location3))
                {
                    var nodeKey = $"L3_{asset.Location3}";
                    if (!processedNodes.Contains(nodeKey))
                    {
                        hierarchyItems.Add(new HierarchyItem
                        {
                            ID = "",
                            Description = asset.Location3,
                            Parent_ID = currentParent,
                            Level = nextLevel,
                            Children = new List<HierarchyItem>()
                        });
                        processedNodes.Add(nodeKey);
                    }
                    currentParent = asset.Location3;
                    nextLevel++;
                }

                // Process Location4
                if (!string.IsNullOrEmpty(asset.Location4))
                {
                    var nodeKey = $"L4_{asset.Location4}";
                    if (!processedNodes.Contains(nodeKey))
                    {
                        hierarchyItems.Add(new HierarchyItem
                        {
                            ID = "",
                            Description = asset.Location4,
                            Parent_ID = currentParent,
                            Level = nextLevel,
                            Children = new List<HierarchyItem>()
                        });
                        processedNodes.Add(nodeKey);
                    }
                    currentParent = asset.Location4;
                    nextLevel++;
                }

                // Process Parent_ID if exists
                if (!string.IsNullOrEmpty(asset.Parent_ID))
                {
                    var nodeKey = $"P_{asset.Parent_ID}";
                    if (!processedNodes.Contains(nodeKey))
                    {
                        var parentAsset = assetIdLookup[asset.Parent_ID];
                        hierarchyItems.Add(new HierarchyItem
                        {
                            ID = asset.Parent_ID,
                            Description = parentAsset.AssetDescription,
                            Parent_ID = currentParent,
                            Level = nextLevel,
                            Children = new List<HierarchyItem>(),
                            IsAssetOrParent = true
                        });
                        processedNodes.Add(nodeKey);
                    }
                    currentParent = asset.Parent_ID;
                    nextLevel++;
                }

                // Process Asset_ID (always last level if present)
                var assetNodeKey = $"A_{asset.Asset_ID}";
                if (!processedNodes.Contains(assetNodeKey))
                {
                    hierarchyItems.Add(new HierarchyItem
                    {
                        ID = asset.Asset_ID,
                        Description = asset.AssetDescription,
                        Parent_ID = currentParent,
                        Level = nextLevel,
                        Children = new List<HierarchyItem>(),
                        IsAssetOrParent = true
                    });
                    processedNodes.Add(assetNodeKey);
                }
            }

            // Build the hierarchy relationships
            BuildHierarchyRelationships(hierarchyItems);

            // Return only root level items
            return hierarchyItems.Where(h => string.IsNullOrEmpty(h.Parent_ID)).ToList();
        }

        private void BuildHierarchyRelationships(List<HierarchyItem> items)
        {
            var lookup = items.ToLookup(i => i.Parent_ID);
            foreach (var item in items)
            {
                item.Children = lookup[item.Description].ToList();
            }
        }

        private void ValidateAssetData(List<Asset> assets)
        {
            // Check for duplicate Asset_IDs
            var duplicateAssetIds = assets.GroupBy(a => a.Asset_ID)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            if (duplicateAssetIds.Any())
            {
                throw new Exception($"Duplicate Asset_IDs found: {string.Join(", ", duplicateAssetIds)}");
            }

            // Check for invalid Parent_IDs
            var invalidParentIds = assets
                .Where(a => !string.IsNullOrEmpty(a.Parent_ID))
                .Where(a => !assets.Any(asset => asset.Asset_ID == a.Parent_ID))
                .Select(a => a.Parent_ID)
                .Distinct();
            if (invalidParentIds.Any())
            {
                throw new Exception($"Invalid Parent_IDs found: {string.Join(", ", invalidParentIds)}");
            }
        }


        private void GenerateAMPModel(List<HierarchyItem> hierarchyData)
        {
            var ampModelDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = "AMP_Model.xlsx"
            };

            if (ampModelDialog.ShowDialog() != true)
                return;

            var progress = new Progress<int>(value =>
            {
                ProgressBar.Value = value;
                StatusText.Text = $"Generating AMP Model: {value}%";
            });

            // Create expenditure list
            var expenditures = new List<Expenditure>();  // TODO: Populate this with actual expenditure data

            downloadService.DownloadAMPModel(
                uploadedAssetDataList,
                uploadedClassDataList,
                hierarchyData,
                expenditures,
                uploadedMaintenanceStrategiesList,
                uploadedMaintenanceProceduresList,
                ProjectStartDate.Value,
                ProjectEndDate.Value,
                ampModelDialog.FileName,
                progress);

            MessageBox.Show(
                "AMP Model has been generated successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            StatusText.Text = "AMP Model generation completed!";
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
            _isLeaveCorrectiveSelected = false;
            _isAdjustCorrectiveSelected = false;
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
            if (!YesButton.IsEnabled) return;
            
            YesButton.Style = (Style)FindResource("SelectedResetButtonStyle");
            NoButton.Style = (Style)FindResource("ActionButtonStyle");
            
            _isAdjustCorrectiveSelected = true;
            _isLeaveCorrectiveSelected = false;
            _isFlatModeSelected = false;
        }

        // Handler for Leave Corrective method
        private void LeaveCorrective_Click(object sender, RoutedEventArgs e)
        {
            if (!NoButton.IsEnabled) return;
            
            NoButton.Style = (Style)FindResource("SelectedResetButtonStyle");
            YesButton.Style = (Style)FindResource("ActionButtonStyle");
            
            _isLeaveCorrectiveSelected = true;
            _isAdjustCorrectiveSelected = false;
            _isFlatModeSelected = false;
        }

        private void DownladHierarchy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = "HierarchyTemplate.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    downloadService.DownloadHierarchyTemplate(saveFileDialog.FileName);
                    MessageBox.Show(
                        "Hierarchy template downloaded successfully! Please fill it out and upload it back.",
                        "Download Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error downloading hierarchy template: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void UploadHierarchy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    // Create empty list since hierarchy is independent of asset IDs
                    var assetIds = new List<string>();
                    hierarchyData = downloadService.ValidateHierarchyTemplate(openFileDialog.FileName, assetIds);
                    
                    // Show preview of hierarchy with download button
                    ShowHierarchyPreviewWithDownload(hierarchyData);

                    MessageBox.Show("Hierarchy template uploaded and validated successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error uploading hierarchy template: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ShowHierarchyPreviewWithDownload(List<HierarchyItem> hierarchyData, Action onCloseCallback = null)
        {
            // Create preview window
            var previewWindow = new Window
            {
                Title = "Hierarchy Preview",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            // Add close callback if provided
            if (onCloseCallback != null)
            {
                previewWindow.Closed += (s, e) => onCloseCallback();
            }

            // Create main grid
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create preview text box
            var textBox = new TextBox
            {
                Text = downloadService.GenerateHierarchyPreview(hierarchyData),
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                Margin = new Thickness(5)
            };
            Grid.SetRow(textBox, 0);
            grid.Children.Add(textBox);

            // Create button panel
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };
            Grid.SetRow(buttonPanel, 1);

            // Create download hierarchy button
            var downloadButton = new Button
            {
                Content = "Download Hierarchy",
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(5)
            };
            
            downloadButton.Click += (s, e) =>
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = "BuiltHierarchy.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        downloadService.SaveHierarchyToFile(hierarchyData, saveFileDialog.FileName);
                        MessageBox.Show(
                            "Hierarchy has been successfully saved!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error saving hierarchy: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            };
            buttonPanel.Children.Add(downloadButton);

            // Create close button
            var closeButton = new Button
            {
                Content = "Close",
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(5)
            };
            closeButton.Click += (s, e) => previewWindow.Close();
            buttonPanel.Children.Add(closeButton);

            // Add button panel to grid
            grid.Children.Add(buttonPanel);

            // Show preview window
            previewWindow.Content = grid;
            previewWindow.ShowDialog();
        }

    }
}
