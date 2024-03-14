using System.CommandLine;

var outputOption = new Option<FileInfo>(new string[] { "--output", "-o" }, "file path and name");
var languageOption = new Option<string>(new string[] { "--language", "-l" }, "");
var noteOption = new Option<bool>(new string[] { "--note", "-n" }, "Whether to include source code comments in the bundle file");
var authorOption = new Option<string>(new string[] { "--author", "-a" }, "Name of the creator of the file");
var removeEmptyLinesOption = new Option<bool>(new string[] { "--remove-empty-lines", "-r" }, "Remove empty lines from the source code");
var sortOption = new Option<string>(new string[] { "--sort", "-s" }, "Sort order (name/type)");

var bundleCommand = new Command("bundle", "Bundle code files to a single file");

bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(authorOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(sortOption);

bundleCommand.SetHandler((output, languages, note, author, removeEmptyLines, sort) =>
{
    try
    {
        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), ".", SearchOption.AllDirectories)
            .Where(file => IsToCopy(file, languages))
            .Where(file => !Path.GetDirectoryName(file).ToLower().Contains("bin") &&
                  !Path.GetDirectoryName(file).ToLower().Contains("debug"));
        if (sort != null)
        {
            if (sort?.ToLower() == "type")
            {
                files = files.OrderBy(file => GetLanguage(Path.GetExtension(file))).ToArray();
            }
            else
            {
                files = files.OrderBy(file => Path.GetFileName(file)).ToArray();
            }
        }
        using (var outputFileStream = File.CreateText(output.FullName))
        {
            if (author != null)
                outputFileStream.WriteLine($"// Created by: {author}");

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                if (note)
                {
                    outputFileStream.WriteLine($"// Source code from: {relativePath}");
                }
                using (var fileStreamReader = File.OpenText(file))
                {
                    string line;
                    while ((line = fileStreamReader.ReadLine()) != null)
                    {
                        if (!removeEmptyLines || !string.IsNullOrWhiteSpace(line))
                        {
                            outputFileStream.WriteLine(line);
                        }
                    }
                }
                outputFileStream.WriteLine();
            }
        }
        Console.WriteLine($"Files bundled and saved at: {output.FullName}");
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: File path is invalid");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: the input not valid!");
    }
}, outputOption, languageOption, noteOption, authorOption, removeEmptyLinesOption, sortOption);

var createRspCommand = new Command("create-rsp", "Create a response file with a ready command");
createRspCommand.AddOption(outputOption);
createRspCommand.AddOption(languageOption);
createRspCommand.AddOption(noteOption);
createRspCommand.AddOption(authorOption);
createRspCommand.AddOption(removeEmptyLinesOption);
createRspCommand.AddOption(sortOption);

createRspCommand.SetHandler((output, languages, note, author, removeEmptyLines, sort) =>
{
    try
    {
        output = PromptForFileInfoOption("Output file path and name: ", output);
        languages = PromptForStringOption("Programming languages (comma-separated): ", languages);
        note = PromptForBoolOption("Include source code comments (true/false): ", note);
        author = PromptForStringOption("Name of the creator of the file: ", author);
        removeEmptyLines = PromptForBoolOption("Remove empty lines from the source code (true/false): ", removeEmptyLines);
        sort = PromptForStringOption("Sort order (name/type): ", sort);

        var fullCommand = $"bundle --output {output} --language {languages} --note {note} --author {author} --remove-empty-lines {removeEmptyLines} --sort {sort}";

        File.WriteAllText("response_file.rsp", fullCommand);
        Console.WriteLine("Response file created successfully: response_file.rsp");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}, outputOption, languageOption, noteOption, authorOption, removeEmptyLinesOption, sortOption);

var rootCommand = new RootCommand("Root command for bundle CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args);

static bool IsToCopy(string path, string Language)
{
    if (Language.Contains("all"))
    {
        return GetLanguage(Path.GetExtension(path)) != "";
    }

    return Language.Contains(GetLanguage(Path.GetExtension(path)));
}

static string GetLanguage(string pathExtension)
{
    switch (pathExtension)
    {
        case ".cs":
            return "csharp";
        case ".sql":
            return "sql";
        case ".html":
            return "html";
        case ".js":
            return "javascript";
        case ".py":
            return "python";
        case ".java":
            return "java";
        case ".cpp":
            return "cpp";
        case ".ts":
            return "typescript";
        case ".asm":
            return "assembly";
        case ".c":
            return "c";
        case ".jsx":
            return "react";
        default:
            return "";
    }
}

static string PromptForStringOption(string prompt, string defaultValue)
{
    Console.Write(prompt);
    string userInput = Console.ReadLine();
    return string.IsNullOrWhiteSpace(userInput) ? defaultValue : userInput;
}

static bool PromptForBoolOption(string prompt, bool defaultValue)
{
    Console.Write(prompt);
    string userInput = Console.ReadLine();
    return string.IsNullOrWhiteSpace(userInput) ? defaultValue : bool.Parse(userInput);
}

static FileInfo PromptForFileInfoOption(string prompt, FileInfo defaultValue)
{
    Console.Write(prompt);
    string userInput = Console.ReadLine();
    return string.IsNullOrWhiteSpace(userInput) ? defaultValue : new FileInfo(userInput);
}



