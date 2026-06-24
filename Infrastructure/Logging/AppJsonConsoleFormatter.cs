using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Medium.Api.Infrastructure.Logging;

public sealed class AppJsonConsoleFormatter : ConsoleFormatter
{
  public const string FormatterName = "app-json";
  private readonly IOptionsMonitor<ConsoleFormatterOptions> _options;

  public AppJsonConsoleFormatter(IOptionsMonitor<ConsoleFormatterOptions> options)
      : base(FormatterName)
  {
    _options = options ?? throw new ArgumentNullException(nameof(options));
  }

  public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
  {
    if (logEntry.State is IEnumerable<KeyValuePair<string, object>> stateProperties)
    {
      var logData = stateProperties
          .Where(p => p.Key != "{OriginalFormat}")
          .ToDictionary(p => p.Key, p => Normalize(p.Value));

      if (logData.Count > 0)
      {
        object targetStructure = logData;

        var json = JsonSerializer.Serialize(targetStructure, new JsonSerializerOptions
        {
          WriteIndented = true,
          Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        textWriter.WriteLine(json);
        return;
      }
    }

    textWriter.WriteLine(logEntry.Formatter(logEntry.State, logEntry.Exception));
  }

  private static object? Normalize(object? value)
  {
    if (value == null)
      return null;

    if (value is string || value.GetType().IsPrimitive)
      return value;

    if (value is DateTime dt)
      return dt.ToString("O");

    if (value is IEnumerable enumerable && value is not string)
    {
      var list = new List<object?>();

      foreach (var item in enumerable)
      {
        list.Add(Normalize(item));
      }

      return list;
    }

    var type = value.GetType();

    if (type.IsClass && type != typeof(string))
    {
      var dict = new Dictionary<string, object?>();

      foreach (var prop in type.GetProperties())
      {
        if (!prop.CanRead) continue;

        try
        {
          var propValue = prop.GetValue(value);
          dict[prop.Name] = Normalize(propValue);
        }
        catch
        {
          dict[prop.Name] = null;
        }
      }

      return dict;
    }

    return value.ToString();
  }

}