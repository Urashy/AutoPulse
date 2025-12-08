using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorAutoPulse.ViewModel
{
    public class FileUploadViewModel
    {
        // --- CONFIGURATION ---
        public const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        public const int MaxFiles = 5;

        private readonly string[] AllowedExtensions = 
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp",
            ".pdf",
            ".doc", ".docx",
            ".txt"
        };

        public string AcceptedFileFormats => string.Join(",", AllowedExtensions);

        // --- ÉTAT ---
        public List<IBrowserFile> SelectedFiles { get; private set; } = new();
        public string ErrorMessage { get; private set; } = "";

        // --- EVENTS ---
        private Func<List<IBrowserFile>, Task>? _onFilesSelectedCallback;

        public void SetOnFilesSelectedCallback(Func<List<IBrowserFile>, Task> callback)
        {
            _onFilesSelectedCallback = callback;
        }

        // --- MÉTHODES PUBLIQUES ---

        public async Task HandleFileSelection(InputFileChangeEventArgs e)
        {
            ErrorMessage = "";

            var newFiles = e.GetMultipleFiles(MaxFiles);

            foreach (var file in newFiles)
            {
                var extension = Path.GetExtension(file.Name).ToLowerInvariant();

                // Vérification extension
                if (!AllowedExtensions.Contains(extension))
                {
                    ErrorMessage = $"Extension non autorisée : {extension}";
                    continue;
                }

                // Vérification taille
                if (file.Size > MaxFileSize)
                {
                    ErrorMessage = $"Le fichier {file.Name} dépasse 10 MB";
                    continue;
                }

                // Vérification nombre
                if (SelectedFiles.Count >= MaxFiles)
                {
                    ErrorMessage = $"Maximum {MaxFiles} fichiers autorisés";
                    break;
                }

                // Éviter les doublons
                if (!SelectedFiles.Any(f => f.Name == file.Name && f.Size == file.Size))
                {
                    SelectedFiles.Add(file);
                }
            }

            // Notifier le composant parent
            if (_onFilesSelectedCallback != null)
            {
                await _onFilesSelectedCallback.Invoke(SelectedFiles);
            }
        }

        public async Task RemoveFile(int index)
        {
            if (index >= 0 && index < SelectedFiles.Count)
            {
                SelectedFiles.RemoveAt(index);

                // Notifier le composant parent
                if (_onFilesSelectedCallback != null)
                {
                    await _onFilesSelectedCallback.Invoke(SelectedFiles);
                }
            }
        }

        public void ClearError()
        {
            ErrorMessage = "";
        }

        public void ClearAllFiles()
        {
            SelectedFiles.Clear();
            ErrorMessage = "";
        }

        // --- HELPERS ---

        public string GetFileIcon(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "📄",
                ".doc" or ".docx" => "📝",
                ".txt" => "📃",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "🖼️",
                _ => "📎"
            };
        }

        public string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024 * 1024)
                return $"{bytes / 1024:F1} KB";

            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}