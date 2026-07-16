#nullable enable
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Entity;

public class ScriptConfig(Version? bootstrapVersion, string? configFileName, ConfigPath? configPath, JsonSchemaPath? schemaPath, FunctionPath functionPath, string[] functions, ModulePath modulePath, List<RequiredModule> modules, bool logEnabled, LogPath? logPath, bool showShellOutput, bool verbose, bool debug)
{
    public Version? BootstrapVersion { get; private set; } = bootstrapVersion;

    public string? ConfigFileName { get; private set; } = configFileName;
    public ConfigPath? ConfigPath { get; private set; } = configPath;
    public JsonSchemaPath? SchemaPath { get; private set; } = schemaPath;

    public FunctionPath FunctionPath { get; private set; } = functionPath;
    public string[] Functions { get; private set; } = functions;

    public ModulePath ModulePath { get; private set; } = modulePath;
    public List<RequiredModule> Modules { get; private set; } = modules;

    public bool LogEnabled { get; private set; } = logEnabled;
    public LogPath? LogPath { get; private set; } = logPath;
    public bool ShowShellOutput { get; private set; } = showShellOutput;

    public bool Verbose { get; private set; } = verbose;
    public bool Debug { get; private set; } = debug;
}