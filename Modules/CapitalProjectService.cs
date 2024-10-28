using System;
using System.Collections.Generic;
using System.IO;
using CoolMangoes.Models;
using Microsoft.Win32;

namespace CoolMangoes.Modules
{
    public class CapitalProjectService
    {
        public List<CapitalProject> UploadedCapitalProjects { get; private set; }

        public CapitalProjectService()
        {
            UploadedCapitalProjects = new List<CapitalProject>();
        }


        public void UploadCapitalProjectsData()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                UploadedCapitalProjects = LoadCapitalProjectsData(filePath);
            }
        }

        private List<CapitalProject> LoadCapitalProjectsData(string filePath)
        {
            var capitalProjects = new List<CapitalProject>();

            try
            {
                var lines = File.ReadAllLines(filePath);

                if (lines.Length <= 1)
                {
                    throw new Exception("The file is empty or only contains headers.");
                }

                // Assuming the first line contains headers
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var fields = line.Split(',');

                    if (fields.Length >= 10)
                    {
                        var project = new CapitalProject
                        {
                            Location1 = fields[0],
                            Location2 = fields[1],
                            Location3 = fields[2],
                            Location4 = fields[3],
                            Asset_ID = fields[4],
                            ProjectCategory = fields[5],
                            ProjectTitle = fields[6],
                            ProjectCost = double.TryParse(fields[7], out double cost) ? cost : 0.0,
                            ProjectStartYear = int.TryParse(fields[8], out int startYear) ? startYear : 0,
                            ProjectEndYear = int.TryParse(fields[9], out int endYear) ? endYear : 0
                        };

                        capitalProjects.Add(project);
                    }
                    else
                    {
                        // Handle lines with incorrect number of fields
                        Console.WriteLine($"Skipping line {i + 1} due to incorrect number of fields.");
                    }
                }

                return capitalProjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading Capital Projects data: {ex.Message}");
                throw; // Re-throw the exception to handle it in the calling method
            }
        }
    }
}
