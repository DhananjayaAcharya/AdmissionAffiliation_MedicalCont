namespace Medical_Affiliation.Services;

public sealed class CollegeLogoStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly IConfiguration _configuration;
    private readonly ILogger<CollegeLogoStorage> _logger;

    public CollegeLogoStorage(
        IConfiguration configuration,
        ILogger<CollegeLogoStorage> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> SaveAsync(
        Guid logoGuid,
        string extension,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Unsupported college logo format.", nameof(extension));
        }

        var failures = new List<Exception>();
        foreach (var root in GetStorageRoots())
        {
            var filePath = Path.Combine(root, logoGuid.ToString("N") + extension.ToLowerInvariant());
            var created = false;
            var completed = false;
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
                created = true;
                await file.CopyToAsync(destination, cancellationToken);
                completed = true;
                return filePath;
            }
            catch (IOException ex)
            {
                failures.Add(ex);
                _logger.LogWarning(ex, "Unable to save college logo to {StorageRoot}.", root);
            }
            catch (UnauthorizedAccessException ex)
            {
                failures.Add(ex);
                _logger.LogWarning(ex, "Access denied while saving college logo to {StorageRoot}.", root);
            }
            finally
            {
                if (created && !completed)
                {
                    TryDelete(filePath);
                }
            }
        }

        throw new IOException("The college logo could not be saved to either configured drive.", new AggregateException(failures));
    }

    public string? GetExistingLogoPath(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return null;
        }

        string fullStoredPath;
        try
        {
            fullStoredPath = Path.GetFullPath(storedPath);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }

        var extension = Path.GetExtension(fullStoredPath);
        if (!AllowedExtensions.Contains(extension)
            || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fullStoredPath), "N", out _))
        {
            return null;
        }

        return GetStorageRoots()
            .Select(root => IsWithinRoot(fullStoredPath, root) ? fullStoredPath : null)
            .FirstOrDefault(path => path is not null && File.Exists(path));
    }

    public void DeleteManagedLogo(string? storedPath)
    {
        var path = GetExistingLogoPath(storedPath);
        if (path is not null)
        {
            TryDelete(path);
        }
    }

    private string[] GetStorageRoots()
    {
        var roots = new[]
        {
            _configuration["CollegeLogos:StoragePathD"] ?? @"D:\RGUHS\CollegeLogos",
            _configuration["CollegeLogos:StoragePathE"] ?? @"E:\RGUHS\CollegeLogos"
        };

        return roots.Select(root =>
        {
            var fullPath = Path.GetFullPath(root);
            var driveRoot = Path.GetPathRoot(fullPath);
            if (driveRoot is null ||
                !(driveRoot.Equals(@"D:\", StringComparison.OrdinalIgnoreCase)
                  || driveRoot.Equals(@"E:\", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("College logo storage paths must be on the D: or E: drive.");
            }

            return fullPath;
        }).ToArray();
    }

    private static bool IsWithinRoot(string path, string root)
    {
        var normalizedRoot = Path.EndsInDirectorySeparator(root)
            ? root
            : root + Path.DirectorySeparatorChar;
        return path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private void TryDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Could not delete old college logo {FilePath}.", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied while deleting old college logo {FilePath}.", filePath);
        }
    }
}
