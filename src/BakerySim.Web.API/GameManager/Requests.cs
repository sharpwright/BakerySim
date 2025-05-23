using System;

namespace BakerySim.Web.API.GameManager;

public record StartGameRequest(string GameName);
public record UpdateGameRequest(Guid GameId, string GameName);