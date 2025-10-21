// Phase1 Step4 — Skyworks V5
namespace Skyworks.AgentComm;

/// <summary>
/// Interface for Agent Communication System.
/// Coordinates communication between Spark and Copilot agents.
/// </summary>
public interface IAgentComm
{
    /// <summary>
    /// Pings the agent communication system.
    /// </summary>
    /// <returns>A task representing the async operation with ping response.</returns>
    Task<string> PingAsync();

    /// <summary>
    /// Synchronizes data between agents.
    /// </summary>
    /// <param name="agentName">The name of the agent to sync with.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation with sync status.</returns>
    Task<string> SyncAsync(string agentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message to a specific agent.
    /// </summary>
    /// <param name="agentName">The target agent name.</param>
    /// <param name="message">The message payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation with response.</returns>
    Task<string> SendMessageAsync(string agentName, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current coordination status.
    /// </summary>
    /// <returns>A task representing the async operation with status information.</returns>
    Task<Dictionary<string, object>> GetStatusAsync();
}