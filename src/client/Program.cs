using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using Microsoft.Extensions.AI;
using OpenAI;
using System.Text;

// Connect to an MCP server
Console.WriteLine("Connecting client to MCP server");

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "helpTroubleshooter",
    Command = "dotnet",
    Arguments = ["run", "--project", "../../../../server/McpServer.csproj", "--no-build"],
});

var client = await McpClientFactory.CreateAsync(clientTransport);

var tools = await client.ListToolsAsync();

var model = "gpt-4o-mini";

using IChatClient chatClient =
    new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")).GetChatClient(model).AsIChatClient()
    .AsBuilder().UseFunctionInvocation().Build();

List<ChatMessage> messages = [];

while (true)
{
    Console.Write("Q: ");

    messages.Add(new(ChatRole.User, Console.ReadLine()));

    List<ChatResponseUpdate> updates = [];

    await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
    {
        Console.Write(update);
        updates.Add(update);
    }

    Console.WriteLine();

    messages.AddMessages(updates);
}
