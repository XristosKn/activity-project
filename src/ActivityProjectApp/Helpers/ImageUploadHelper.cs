using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ActivityProjectApp.Helpers
{
    public static class ImageUploadHelper
    {
        public static async Task<string> SavePickedImageAsync(
            IStorageFile pickedFile,
            string itemFolderName)
        {
            string? sourcePath = pickedFile.Path.LocalPath;

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return string.Empty;
            }

            string uploadsRootFolder = Path.Combine(
                AppContext.BaseDirectory,
                "UploadedImages",
                itemFolderName);

            Directory.CreateDirectory(uploadsRootFolder);

            string originalFileName = Path.GetFileName(sourcePath);
            string safeFileName = $"{Guid.NewGuid():N}_{originalFileName}";
            string destinationPath = Path.Combine(uploadsRootFolder, safeFileName);

            File.Copy(sourcePath, destinationPath, true);

            return destinationPath;
        }

        public static bool IsSupportedImageFile(IStorageFile file)
        {
            string extension = Path.GetExtension(file.Name).ToLower();

            return extension == ".jpg" ||
                   extension == ".jpeg" ||
                   extension == ".png" ||
                   extension == ".webp";
        }

        public static List<IStorageFile> FilterSupportedImageFiles(
            IReadOnlyList<IStorageFile> files)
        {
            List<IStorageFile> supportedFiles = new List<IStorageFile>();

            foreach (IStorageFile file in files)
            {
                if (IsSupportedImageFile(file))
                {
                    supportedFiles.Add(file);
                }
            }

            return supportedFiles;
        }
    }
}