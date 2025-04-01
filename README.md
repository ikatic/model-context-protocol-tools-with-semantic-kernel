# MCP Server with Semantic Kernel Integration

This solution demonstrates how to integrate Model Context Protocol (MCP) tools with Microsoft Semantic Kernel and it's based on https://devblogs.microsoft.com/semantic-kernel/integrating-model-context-protocol-tools-with-semantic-kernel-a-step-by-step-guide/

The example is a complete solution and it shows how to:

1. Connect to an MCP Server using ModelContextProtocol
2. Retrieve the list of tools the MCP Server makes available
3. Convert the MCP tools to Semantic Kernel functions
4. Invoke the MCP tools from Semantic Kernel in response to LLM function calling requests

## Prerequisites

- .NET 8.0 SDK or later
- Node.js and npm (for running the GitHub MCP server)
- OpenAI API key

## Setup

1. Clone this repository
2. Add your OpenAI API key to user secrets:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here"
   ```

## Running the Solution

1. Navigate to the project directory:
   ```bash
   cd MCPServerWithSK.ConsoleApp
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

The application will:
1. Connect to the GitHub MCP server
2. List all available GitHub tools
3. Convert these tools to Semantic Kernel functions
4. Interact with the "agent" by asking questions and git hub actions, e.g. "Tell me more about ikatic/model-context-protocol-tools-with-semantic-kernel repository."

## How It Works

The solution uses the ModelContextProtocol package to connect to a GitHub MCP server, which provides access to various GitHub operations. These operations are then converted into Semantic Kernel functions that can be called by the LLM.

The main components are:
- Configuration setup for OpenAI
- MCP client creation and connection
- Tool retrieval and conversion to Kernel functions
- Automatic function calling setup
- Prompt execution

## Notes

- The GitHub MCP server is started using `npx` and runs as a separate process
- The solution uses the stdio transport type for communication with the MCP server
- OpenAI's o3-mini-2025-01-31 model is used by default, but can be configured through settings 