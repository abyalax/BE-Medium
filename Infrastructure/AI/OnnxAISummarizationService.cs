using System.Text.RegularExpressions;

using Microsoft.ML.OnnxRuntimeGenAI;

namespace Medium.Api.Infrastructure.AI;

public interface IOnnxAISummarizationService
{
  Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken = default);
  Task InitializeAsync(CancellationToken cancellationToken = default);
}

public class OnnxAISummarizationService(
  ILogger<OnnxAISummarizationService> logger,
  IConfiguration configuration) : IOnnxAISummarizationService, IDisposable
{
  private readonly ILogger<OnnxAISummarizationService> _logger = logger;
  private readonly IConfiguration _configuration = configuration;
  private Model? _model;
  private bool _isInitialized = false;

  public void Initialize(CancellationToken cancellationToken = default)
  {
    if (_isInitialized) return;

    try
    {
      var modelPath = _configuration["AppSettings:AI:ModelPath"] ?? throw new InvalidOperationException("AI Model Path must be provided");

      // ONNX Runtime GenAI expects a directory path, not a file path
      // The directory should contain genai_config.json and the model files
      if (!Directory.Exists(modelPath))
        throw new DirectoryNotFoundException($"Model directory not found at {modelPath}");

      var genaiConfigPath = Path.Combine(modelPath, "genai_config.json");
      if (!File.Exists(genaiConfigPath))
        throw new FileNotFoundException($"genai_config.json not found in model directory at {genaiConfigPath}");

      _logger.LogInformation("Loading ONNX model from directory {ModelPath}", modelPath);

      // Create model - ONNX Runtime GenAI expects directory path
      _model = new Model(modelPath);
      _logger.LogInformation("ONNX model loaded successfully");

      _isInitialized = true;
      _logger.LogInformation("AI Summarization Service initialized successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize AI Summarization Service");
      throw;
    }
  }

  public Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    return Task.Run(() => Initialize(cancellationToken), cancellationToken);
  }

  public async Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken = default)
  {
    if (!_isInitialized)
      await InitializeAsync(cancellationToken);

    if (_model == null)
      throw new InvalidOperationException("Model is not initialized");

    return await Task.Run(() => GenerateSummarySync(content, cancellationToken), cancellationToken);
  }

  private string GenerateSummarySync(string content, CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Generating summary for content of length {Length}", content.Length);

      // Create tokenizer
      var tokenizer = new Tokenizer(_model!);
      // Create the prompt for summarization
      var prompt = CreateSummarizationPrompt(content);
      // Tokenize the prompt
      var sequences = tokenizer.Encode(prompt);

      // Create generator parameters
      var generatorParams = new GeneratorParams(_model!);
      generatorParams.SetSearchOption("max_length", 512);
      generatorParams.SetSearchOption("temperature", 0.7f);
      generatorParams.SetSearchOption("top_p", 0.9f);
      generatorParams.SetSearchOption("do_sample", true);

      // Use reflection to call SetInputSequences if available
      var setInputSequencesMethod = generatorParams.GetType().GetMethod("SetInputSequences");
      if (setInputSequencesMethod != null)
      {
        setInputSequencesMethod.Invoke(generatorParams, new object[] { sequences });
      }
      else
      {
        _logger.LogWarning("SetInputSequences method not available, trying SetInputIDs");
        // Try alternative approach
        var setInputIdsMethod = generatorParams.GetType().GetMethod("SetInputIDs");
        if (setInputIdsMethod != null)
        {
          var inputIds = sequences[0].ToArray();
          setInputIdsMethod.Invoke(generatorParams, new object[] { inputIds, (ulong)inputIds.Length, (ulong)1 });
        }
        else
        {
          throw new InvalidOperationException("Neither SetInputSequences nor SetInputIDs method available");
        }
      }

      // Generate the summary using manual token generation
      using var generator = new Generator(_model!, generatorParams);
      var outputTokens = new List<int>();

      while (!generator.IsDone())
      {
        cancellationToken.ThrowIfCancellationRequested();
        generator.GenerateNextToken();
        var outputSequence = generator.GetSequence(0);
        var outputSequenceArray = outputSequence.ToArray();
        var lastToken = outputSequenceArray[^1];
        outputTokens.Add(lastToken);

        // Break if we've generated too many tokens
        if (outputTokens.Count > 512)
        {
          _logger.LogWarning("Summary generation exceeded max token count, stopping early");
          break;
        }
      }

      // Decode the output tokens
      var summary = tokenizer.Decode(outputTokens.ToArray());

      // Clean up the summary (remove the prompt part if included)
      summary = CleanSummary(summary);

      _logger.LogInformation("Generated summary of length {Length}", summary.Length);
      return summary;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating summary");
      throw;
    }
  }

  private static string CreateSummarizationPrompt(string content)
  {
    // Truncate content if it's too long for the model context
    var maxContentLength = 2000; // Leave room for the prompt and summary
    if (content.Length > maxContentLength)
      content = string.Concat(content.AsSpan(0, maxContentLength), "...");

    // Use the Phi-3 chat template format (without special characters for now)
    return "You are a helpful assistant that creates concise summaries of articles. " +
           "Please summarize the following article in 2-3 sentences: " + content;
  }

  private static string CleanSummary(string summary)
  {
    // Trim whitespace
    summary = summary.Trim();
    // Remove any extra newlines
    summary = Regex.Replace(summary, @"\n{3,}", "\n\n");
    return summary;
  }

  public void Dispose() => _model?.Dispose();
}
