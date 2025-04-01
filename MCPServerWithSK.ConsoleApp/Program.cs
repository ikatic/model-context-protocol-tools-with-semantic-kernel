using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol;
using ModelContextProtocol.Client;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        // Prepare and build kernel
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

        var openAiKey = config["OpenAI:ApiKey"];
        if (openAiKey is not null)
        {
            builder.Services.AddOpenAIChatCompletion(
                serviceId: "openai",
                modelId: config["OpenAI:ChatModelId"] ?? "o3-mini-2025-01-31",
                apiKey: openAiKey);
        }
        else
        {
            Console.Error.WriteLine("Please provide a valid OpenAI:ApiKey to run this sample.");
            return;
        }

        Kernel kernel = builder.Build();

        // Create an MCPClient for the GitHub server
        var serverConfig = new McpServerConfig
        {
            Id = "github",
            Name = "GitHub",
            TransportType = "stdio",
            TransportOptions = new Dictionary<string, string>
            {
                ["command"] = "npx",
                ["arguments"] = "-y @modelcontextprotocol/server-github",
            }
        };

        await using var mcpClient = await McpClientFactory.CreateAsync(
            serverConfig,
            new McpClientOptions { ClientInfo = new() { Name = "GitHub", Version = "1.0.0" } }).ConfigureAwait(false);

        // Retrieve the list of tools available on the GitHub server
        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
        /*
        foreach (var tool in tools)
        {
            Console.WriteLine($"{tool.Name}: {tool.Description}");
        }
        */
        
        // Convert MCP tools to Kernel functions and add them to the kernel
        kernel.Plugins.AddFromFunctions("GitHub", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

        // Enable automatic function calling
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        Console.WriteLine("\nWelcome to the GitHub MCP Assistant!");
        Console.WriteLine("You can ask questions about GitHub repositories and operations.");
        Console.WriteLine("Type '/bye' to exit.\n");

        // Interactive prompt loop
        while (true)
        {
            Console.Write("\nEnter your prompt (or '/bye' to exit): ");
            var prompt = Console.ReadLine();

            if (string.IsNullOrEmpty(prompt) || prompt.Trim().ToLower() == "/bye")
            {
                Console.WriteLine("\nGoodbye!");
                break;
            }

            try
            {
                var result = await kernel.InvokePromptAsync(prompt, new(executionSettings)).ConfigureAwait(false);
                Console.WriteLine($"\nResponse:\n{result}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}\n");
            }
        }
    }
}

#pragma warning restore SKEXP0001
