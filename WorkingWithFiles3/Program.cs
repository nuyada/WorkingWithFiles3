using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingWithFiles3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path to the desired folder");
            string folderPath = Console.ReadLine();

            try
            {
                long sizeBeforeCleanup = GetFolderSize(folderPath);
                Console.WriteLine($"Size of folder before cleanup: {sizeBeforeCleanup} bytes");

                int deletedFilesCount = Cleanup(folderPath, TimeSpan.FromMinutes(30));
                Console.WriteLine($"{deletedFilesCount} files deleted");

                long sizeAfterCleanup = GetFolderSize(folderPath);
                Console.WriteLine($"Size of folder after cleanup: {sizeAfterCleanup} bytes");

                long freedSpace = sizeBeforeCleanup - sizeAfterCleanup;
                Console.WriteLine($"Freed space: {freedSpace} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadKey();
        }

        static int Cleanup(string folderPath, TimeSpan timeout)
        {
            // Проверяем, что папка существует
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
            }

            // Вычисляем момент времени, который был timeout минут назад
            DateTime offset = DateTime.UtcNow.Subtract(timeout);

            int deletedFilesCount = 0;

            // Перебираем все файлы в папке и удаляем те, которые не использовались более timeout минут
            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                if (IsFileInactive(filePath, offset))
                {
                    File.Delete(filePath);
                    deletedFilesCount++;
                }
            }

            // Перебираем все подпапки и рекурсивно вызываем метод Cleanup для них
            foreach (string subfolderPath in Directory.GetDirectories(folderPath))
            {
                deletedFilesCount += Cleanup(subfolderPath, timeout);

                // Если подпапка пустая, удаляем ее
                if (IsDirectoryEmpty(subfolderPath))
                {
                    Directory.Delete(subfolderPath);
                }
            }

            return deletedFilesCount;
        }

        static bool IsFileInactive(string filePath, DateTime offset)
        {
            // Возвращает true, если файл не использовался более offset минут
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.LastAccessTimeUtc < offset;
        }

        static bool IsDirectoryEmpty(string directoryPath)
        {
            // Возвращает true, если папка пустая
            return Directory.GetFiles(directoryPath).Length == 0 && Directory.GetDirectories(directoryPath).Length == 0;
        }

        static long GetFolderSize(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
            }

            long size = 0;

            // Перебираем все файлы в папке и добавляем их размер к общему размеру
            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                size += fileInfo.Length;
            }

            // Перебираем все подпапки и рекурсивно вызываем метод GetFolderSize для них
            foreach (string subfolderPath in Directory.GetDirectories(folderPath))
            {
                size += GetFolderSize(subfolderPath);
            }

            return size;
        }
    }
}

