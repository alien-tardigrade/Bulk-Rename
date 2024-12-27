using System.Diagnostics;

namespace bulk_rename
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)    
            {
                Console.WriteLine("Usage: bulk-rename <folder-path> <rename|reverse> [excluded-extensions]");
                return;
            }

            var folderPath = args[0];
            var action = args[1].ToLower();
            var logFilePath = Path.Combine(folderPath, $"rename_log_{DateTime.Now:yyyyMMddHHmmss}.txt");
            var excludedExtensions = args.Length > 2 ? args[2].Split(',') : [];

            try
            {
                switch (action)
                {
                    case "rename":
                        RenameFiles(folderPath, logFilePath, excludedExtensions);
                        break;
                    case "reverse":
                        ReverseRenaming(folderPath);
                        break;
                    default:
                        Console.WriteLine("Invalid action. Use 'rename' or 'reverse'.");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError($"An error occurred: {ex.Message}");
            }
        }

        private static void RenameFiles(string folderPath, string logFilePath, string[] excludedExtensions)
        {
            try
            {
                var folderName = new DirectoryInfo(folderPath).Name;
                
                Console.WriteLine("Folder Name: " + folderName);
                
                var files = Directory.GetFiles(folderPath)
                                     .OrderBy(File.GetCreationTime)
                                     .ToArray();
                var totalFiles = files.Length;
                var digits = totalFiles.ToString().Length;
                using (var logFile = new StreamWriter(logFilePath))
                {
                    var sequence = 1;
                    foreach (var file in files)
                    {
                        var extension = Path.GetExtension(file);
                        if (excludedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                        {
                            LogInfo($"Skipped {file} due to excluded extension {extension}");
                            continue;
                        }
                        
                        var paddedSequence = sequence.ToString().PadLeft(digits, '0');
                        var newFileName = $"{folderName}_{paddedSequence}{extension}";
                        var newFilePath = Path.Combine(folderPath, newFileName);

                        logFile.WriteLine($"{Path.GetFileName(file)} -> {newFileName}");
                        File.Move(file, newFilePath);

                        LogInfo($"Renamed {file} to {newFilePath}");
                        sequence++;
                    }
                }
                Console.WriteLine("Files renamed successfully.");
            }
            catch (Exception ex)
            {
                LogError($"An error occurred while renaming files: {ex.Message}");
            }
        }

        private static void ReverseRenaming(string folderPath)
        {
            var logFiles = Directory.GetFiles(folderPath, "rename_log_*.txt")
                .OrderByDescending(File.GetCreationTime)
                .ToArray();
            if (!logFiles.Any())
            {
                Console.WriteLine("Log file not found. Cannot reverse renaming.");
                return;
            }

            foreach (var logFilePath in logFiles)
            {
                try
                {
                    using (var logFile = new StreamReader(logFilePath))
                    {
                        while (logFile.ReadLine() is { } line)
                        {
                            var parts = line.Split(" -> ");
                            if (parts.Length != 2) continue;
                            var oldFileName = parts[0];
                            var newFileName = parts[1];
                            var oldFilePath = Path.Combine(folderPath, oldFileName);
                            var newFilePath = Path.Combine(folderPath, newFileName);

                            if (!File.Exists(newFilePath)) continue;
                            File.Move(newFilePath, oldFilePath);
                            LogInfo($"Reversed {newFilePath} to {oldFilePath}");
                        }
                    }
                    File.Delete(logFilePath);
                    LogInfo($"Deleted log file: {logFilePath}");
                }
                catch (Exception ex)
                {
                    LogError($"An error occurred while reversing renaming: {ex.Message}");
                }
            }
            Console.WriteLine("Files renamed back to original names successfully.");
        }
        
        private static void ReverseRenaming(string folderPath, string logFilePath)
        {
            if (!File.Exists(logFilePath))
            {
                Console.WriteLine("Log file not found. Cannot reverse renaming.");
                return;
            }

            try
            {
                using (var logFile = new StreamReader(logFilePath))
                {
                    while (logFile.ReadLine() is { } line)
                    {
                        var parts = line.Split(" -> ");
                        if (parts.Length != 2) continue;
                        var oldFileName = parts[0];
                        var newFileName = parts[1];
                        var oldFilePath = Path.Combine(folderPath, oldFileName);
                        var newFilePath = Path.Combine(folderPath, newFileName);

                        if (!File.Exists(newFilePath)) continue;
                        File.Move(newFilePath, oldFilePath);
                        LogInfo($"Reversed {newFilePath} to {oldFilePath}");
                    }
                }
                Console.WriteLine("Files renamed back to original names successfully.");
            }
            catch (Exception ex)
            {
                LogError($"An error occurred while reversing renaming: {ex.Message}");
            }
        }

        private static void LogInfo(string message)
        {
            Trace.TraceInformation(message);
            Console.WriteLine(message);
        }

        private static void LogError(string message)
        {
            Trace.TraceError(message);
            Console.WriteLine(message);
        }
    }
}