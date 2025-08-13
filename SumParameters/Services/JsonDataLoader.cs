using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Text.Json;

namespace SumParameters.Services;

public class JsonDataLoader 
{
    private readonly string _fileFullPath;
    private const string CompanyFolderName = "NoNameData";

    public JsonDataLoader(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentNullException(nameof(fileName), "Имя файла не может быть пустым");
        try
        {
            // Получаем путь к директории AppData\Roaming\NoNameData
            string directoryPath = GetOrCreateAppDataDirectory();

            // Проверяем и очищаем имя файла от недопустимых символов
            string safeFileName = GetSafeFileName(fileName);

            // Формируем полный путь к файлу
            _fileFullPath = Path.Combine(directoryPath, safeFileName);
        }
        catch (Exception ex)
        {
            throw new IOException($"Ошибка при инициализации JsonDataLoader: {ex.Message}", ex);
        }
    }

    private string GetOrCreateAppDataDirectory()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string directoryPath = Path.Combine(appDataPath, CompanyFolderName);

        if (Directory.Exists(directoryPath)) return directoryPath;
        try
        {
            Directory.CreateDirectory(directoryPath);
        }
        catch (Exception ex)
        {
            throw new IOException($"Не удалось создать директорию {directoryPath}: {ex.Message}", ex);
        }

        return directoryPath;
    }

    private string GetSafeFileName(string fileName)
    {
        // Удаляем недопустимые символы из имени файла
        string invalidChars = new string(Path.GetInvalidFileNameChars());
        foreach (char invalidChar in invalidChars)
        {
            fileName = fileName.Replace(invalidChar.ToString(), "");
        }

        // Если имя файла не содержит расширение .json, добавляем его
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".json";
        }

        return fileName;
    }

    public string FilePath => _fileFullPath;

    public bool FileExists => File.Exists(_fileFullPath);

    /// <summary>
    /// Создает настройки для JsonSerializer
    /// </summary>
    private JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    /// <summary>
    /// Загружает данные из JSON файла и десериализует их в указанный тип
    /// </summary>
    /// <typeparam name="T">Тип данных для десериализации</typeparam>
    /// <returns>Десериализованный объект или null в случае ошибки</returns>
    public T LoadData<T>() where T : class
    {
        try
        {
            // Проверяем существование файла
            if (!FileExists)
            {
                return null;
            }

            // Проверяем, не пустой ли файл
            var fileInfo = new FileInfo(_fileFullPath);
            if (fileInfo.Length == 0)
            {
                return null;
            }

            // Читаем и десериализуем данные
            string jsonContent;
            using (var streamReader = new StreamReader(_fileFullPath, Encoding.UTF8))
            {
                jsonContent = streamReader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return null;
            }

            // Настройки десериализации
            var options = CreateJsonOptions();

            return JsonSerializer.Deserialize<T>(jsonContent, options);
        }
        catch (JsonException jsonEx)
        {
            string errorMessage = GetDetailedJsonErrorMessage(jsonEx);
            ShowError(errorMessage);
            return null;
        }
        catch (Exception ex)
        {
            string errorMessage = GetDetailedErrorMessage(ex);
            ShowError(errorMessage);
            return null;
        }
    }

    private string GetDetailedJsonErrorMessage(JsonException jsonEx)
    {
        var errorBuilder = new StringBuilder();
        errorBuilder.AppendLine("Ошибка при разборе JSON:");
        errorBuilder.AppendLine($"Файл: {_fileFullPath}");
        errorBuilder.AppendLine($"Позиция: {jsonEx.LineNumber}:{jsonEx.BytePositionInLine}");
        errorBuilder.AppendLine($"Сообщение: {jsonEx.Message}");
        
        return errorBuilder.ToString();
    }

    private string GetDetailedErrorMessage(Exception ex)
    {
        var errorBuilder = new StringBuilder();
        errorBuilder.AppendLine("Ошибка при загрузке данных:");
        errorBuilder.AppendLine($"Файл: {_fileFullPath}");

        var currentEx = ex;
        while (currentEx != null)
        {
            errorBuilder.AppendLine($"- {currentEx.Message}");
            currentEx = currentEx.InnerException;
        }

        return errorBuilder.ToString();
    }

    private void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "Ошибка загрузки данных",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    /// <summary>
    /// Сохраняет данные в JSON файл
    /// </summary>
    /// <typeparam name="T">Тип сохраняемых данных</typeparam>
    /// <param name="data">Данные для сохранения</param>
    public void SaveData<T>(T data) where T : class
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        try
        {
            // Создаем директорию, если она не существует
            string directory = Path.GetDirectoryName(_fileFullPath);
            if (!Directory.Exists(directory))
            {
                if (directory != null) Directory.CreateDirectory(directory);
            }

            // Настройки сериализации
            var options = CreateJsonOptions();

            // Сериализуем данные
            string json = JsonSerializer.Serialize(data, options);

            // Записываем во временный файл
            string tempPath = _fileFullPath + ".tmp";
            using (var streamWriter = new StreamWriter(tempPath, false, Encoding.UTF8))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            // Заменяем оригинальный файл временным
            if (File.Exists(_fileFullPath))
            {
                File.Delete(_fileFullPath);
            }

            File.Move(tempPath, _fileFullPath);
        }
        catch (Exception ex)
        {
            string errorMessage = GetDetailedErrorMessage(ex);
            ShowError($"Ошибка при сохранении данных:\n{errorMessage}");
        }
    }

  
}
