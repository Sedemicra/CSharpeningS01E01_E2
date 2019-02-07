using System;
using System.IO;
using System.IO.Compression;

namespace CSharpeningS01E01_E2
{
    class Program
    {
        static void Main(string[] args)
        {
            string zipFile, extractPath;

            // Loop to give the user a chance to correct Zip archive path insertion mistakes on the fly.
            do
            {
                Console.Write("Insert path to Zip archive: ");
                zipFile = Console.ReadLine();

                // File.Exists should be safe in the context of this exercise 
                // as it returns valid boolean depending on file presence and no exceptions.
                if (!File.Exists(zipFile))
                {
                    Console.WriteLine($"Unable to locate Zip archive at: {zipFile}");
                    Console.WriteLine("Please check your path.");
                    ContinueExitDialog();
                }
                else
                {
                    break;
                }                
            } while (true);

            // Loop to give the user a chance to correct unzipping target location.
            do
            {
                Console.Write("Insert path where to unzip the ZIP archive: ");
                // Accepting both absolute and relative paths - up to the user, how to use it.
                extractPath = Path.Combine(Console.ReadLine(), Path.GetFileNameWithoutExtension(zipFile));

                // Following the recommendation in https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-compress-and-extract-files
                // Normalizes the path.
                extractPath = Path.GetFullPath(extractPath);
                // Ensures that the last character on the extraction path is the directory separator char. 
                // Without this, a malicious zip file could try to traverse outside of the expected extraction path.
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    extractPath += Path.DirectorySeparatorChar;

                if (Directory.Exists(extractPath))
                {
                    Console.WriteLine("The provided directory path is not unique and poses a risk of conflict.");
                    Console.WriteLine("Please use another path.");
                    ContinueExitDialog();
                }
                else
                {
                    break;
                }
            } while (true);            

            // Informing user about the unzipping location in case a relative path was used.
            Console.WriteLine($"Will unzip to: {extractPath}");
            Console.WriteLine("Unzipping...");
            bool unzipStatus;
            try
            {
                ZipFile.ExtractToDirectory(zipFile, extractPath);
                Console.WriteLine("Unzipping process complete.");
                unzipStatus = true;
            }
            // The sheer amount of possible different exceptions makes no sense to specially handle in the context of this exercise.
            catch (Exception ex)
            {
                unzipStatus = false;
                Console.WriteLine($"Something went wrong when attempting to unzip: {ex.Message}");
            }
            
            // Compression ratio calculation needs unzipping to be successful
            if (unzipStatus)
            {
                CompressionRatio(zipFile, extractPath);
            }
            else
            {
                Console.WriteLine("Due to failure in unzipping unable to claculate compression rate.");
            }
                         
            OldestFileAgeInArchive(zipFile);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // Method for determining whether user wishes to proceed or quit.
        static void ContinueExitDialog()
        {
            // Loop until a valid key is used. Not accepting random keys to minimize confusion.
            do
            {
                Console.WriteLine("Press ESC key to exit or ENTER to continue.");
                var nextKey = Console.ReadKey().Key;
                if (nextKey == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
                if (nextKey == ConsoleKey.Enter)
                {
                    break;
                }
            } while (true);
        }

        // Method that calculates the Compression Ratio / Space Savings of Zip archive.
        // Usually Compression Ratio, as the name says, is given as ratio. Space Saving on the other hand can be given as percentage.
        // Source: https://en.wikipedia.org/wiki/Data_compression_ratio & https://sourceforge.net/p/sevenzip/discussion/45798/thread/d58e601c/
        // Since the exercise description is somewhat ambiguous, I'm providing both. 
        static void CompressionRatio(string compressedFilePath, string extractedFilesPath)
        {
            // Declearing varables as double to prevent rounding loss in division later on.
            double compressedSize, uncompressedSize = 0;
                        
            try
            {
                compressedSize = new FileInfo(compressedFilePath).Length;
            }
            // For simplicity sake handle all issues of getting archive size in one general catch.
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong when attempting to retrieve compressed file size: {ex.Message}");
                return;
            }
                        
            try
            {
                foreach (var file in Directory.GetFiles(extractedFilesPath, "*.*", SearchOption.AllDirectories))
                {
                    var f = new FileInfo(file);
                    uncompressedSize += f.Length;
                }
            }
            // For simplicity sake handle all issues of getting unziped files sizes in one general catch.
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong when attempting to retrieve unpacked files size: {ex.Message}");
                return;
            }

            var compressionRate = uncompressedSize / compressedSize;
            Console.WriteLine($"The compression ratio of the Zip archive was: {compressionRate:N2}:1");
            var spaceSaving = (1 - compressedSize / uncompressedSize) * 100;
            Console.WriteLine($"The space saving of The zip arcive was {spaceSaving:N2}%.");            
        }

        // Method that calculates oldest file age in days by date modified.
        // Since the presence of date created depends on the archive and presence of date modified doesn't,
        // I decided to use date modifed as the basis for determining file age.   
        static void OldestFileAgeInArchive(string archivePath)
        {
            int oldestFileAge = 0;
            try
            {
                using (var archive = ZipFile.OpenRead(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var fileAge = DateTime.Now.Subtract(entry.LastWriteTime.Date).Days;
                        if (fileAge > oldestFileAge)
                        {
                            oldestFileAge = fileAge;
                        }
                    }
                }
            }
            // Very general handling of exceptions as this is not relevant within the scope of current task.
            catch (Exception ex)
            {
                Console.WriteLine($"Was unable to determine oldest file age: {ex.Message}");
                return;
            }
            Console.WriteLine($"The oldest file (by last modified date) was last modified {oldestFileAge} days ago.");
        }
    }
}
