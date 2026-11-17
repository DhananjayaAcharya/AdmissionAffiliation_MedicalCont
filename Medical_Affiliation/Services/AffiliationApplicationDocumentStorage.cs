using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services;

public sealed record StoredAffiliationApplicationDocument(string StorageDrive, string FilePath);

public sealed class AffiliationApplicationDocumentStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx"
    };

    private readonly IConfiguration _configuration;
    private readonly ILogger<AffiliationApplicationDocumentStorage> _logger;

    public AffiliationApplicationDocumentStorage(
        IConfiguration configuration,
        ILogger<AffiliationApplicationDocumentStorage> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<StoredAffiliationApplicationDocument> SaveAsync(
        Guid documentGuid,
        string extension,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Only PDF, DOC, and DOCX files are supported.", nameof(extension));
        }

        var failures = new List<Exception>();
        foreach (var root in GetStorageRoots())
        {
            var filePath = Path.Combine(root, documentGuid.ToString("N") + extension.ToLowerInvariant());
            var fileCreated = false;
            var saveCompleted = false;
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(root)!);
                if (!drive.IsReady)
                {
                    throw new IOException($"Storage drive {drive.Name} is not ready.");
                }

                Directory.CreateDirectory(root);
                await using var destination = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);
                fileCreated = true;
                await file.CopyToAsync(destination, cancellationToken);

                saveCompleted = true;
                return new StoredAffiliationApplicationDocument(root[..1].ToUpperInvariant(), filePath);
            }
            catch (IOException ex)
            {
                failures.Add(ex);
                _logger.LogWarning(ex, "Unable to save affiliation application document to {StorageRoot}.", root);
            }
            catch (UnauthorizedAccessException ex)
            {
                failures.Add(ex);
                _logger.LogWarning(ex, "Access denied while saving affiliation application document to {StorageRoot}.", root);
            }
            finally
            {
                if (fileCreated && !saveCompleted)
                {
                    DeletePartialFile(filePath);
                }
            }
        }

        throw new IOException(
            "The document could not be saved to either configured storage drive.",
            new AggregateException(failures));
    }

    public string? GetExistingFilePath(AffiliationApplicationDocument document)
    {
        if (!AllowedExtensions.Contains(document.FileExtension))
        {
            return null;
        }

        foreach (var root in GetStorageRoots())
        {
            var expectedPath = Path.GetFullPath(
                Path.Combine(root, document.DocumentGuid.ToString("N") + document.FileExtension.ToLowerInvariant()));
            string actualPath;
            try
            {
                actualPath = Path.GetFullPath(document.FilePath);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return null;
            }

            if (string.Equals(
                    actualPath,
                    expectedPath,
                    StringComparison.OrdinalIgnoreCase)
                && System.IO.File.Exists(expectedPath))
            {
                return expectedPath;
            }
        }

        return null;
    }

    private string[] GetStorageRoots()
    {
        var roots = new[]
        {
            _configuration["AffiliationApplicationDocuments:StoragePathD"] ?? @"D:\RGUHS\AffiliationDocuments",
            _configuration["AffiliationApplicationDocuments:StoragePathE"] ?? @"E:\RGUHS\AffiliationDocuments"
        };

        return roots.Select(root =>
        {
            var fullPath = Path.GetFullPath(root);
            var driveRoot = Path.GetPathRoot(fullPath);
            if (driveRoot is null ||
                !(driveRoot.Equals(@"D:\", StringComparison.OrdinalIgnoreCase)
                  || driveRoot.Equals(@"E:\", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "Affiliation application document storage paths must be on the D: or E: drive.");
            }

            return fullPath;
        }).ToArray();
    }

    private void DeletePartialFile(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Could not remove incomplete affiliation document {FilePath}.", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied while removing incomplete affiliation document {FilePath}.", filePath);
        }
    }
}
