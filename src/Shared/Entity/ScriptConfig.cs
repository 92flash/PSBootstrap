#nullable enable
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Entity;

public class ScriptConfig(
    Version? bootstrapVersion,
    bool autoResolveRelativeConfigPaths,
    string? configFileName,
    ConfigPath? configPath,
    JsonSchemaPath? schemaPath,
    bool functionsEnabled,
    FunctionPath? functionPath,
    string[] functions,
    bool modulesEnabled,
    ModulePath? modulePath,
    List<RequiredModule> modules,
    bool logEnabled,
    LogPath? logPath,
    bool showShellOutput,
    bool verbose,
    bool debug)
{
    public Version? BootstrapVersion { get; private set; } = bootstrapVersion;

    public bool AutoResolveRelativeConfigPaths { get; private set; } = autoResolveRelativeConfigPaths;
    public string? ConfigFileName { get; private set; } = configFileName;
    public ConfigPath? ConfigPath { get; private set; } = configPath;
    public JsonSchemaPath? SchemaPath { get; private set; } = schemaPath;

    public bool FunctionsEnabled { get; private set; } = functionsEnabled;
    public FunctionPath? FunctionPath { get; private set; } = functionPath;
    public string[] Functions { get; private set; } = functions;

    public bool ModulesEnabled { get; private set; } = modulesEnabled;
    public ModulePath? ModulePath { get; private set; } = modulePath;
    public List<RequiredModule> Modules { get; private set; } = modules;

    public bool LogEnabled { get; private set; } = logEnabled;
    public LogPath? LogPath { get; private set; } = logPath;
    public bool ShowShellOutput { get; private set; } = showShellOutput;

    public bool Verbose { get; private set; } = verbose;
    public bool Debug { get; private set; } = debug;
}