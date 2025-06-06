using System;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.TestKit;
using PastryTycoon.Core.Abstractions.Game;
using PastryTycoon.Data.Recipes;
using PastryTycoon.Core.Grains.Common;
using PastryTycoon.Core.Grains.Game;

namespace PastryTycoon.Core.Grains.UnitTests.Game;

/// <summary>
/// Tests for the GameFactoryGrain actor using Orleans TestKit.
/// </summary>
public class GameFactoryGrainTests : TestKitBase
{
    private readonly Mock<IRecipeRepository> recipeRepositoryMock = new();
    private readonly Mock<ILogger<GameFactoryGrain>> loggerMock = new();
    private readonly Mock<IGuidProvider> guidProviderMock = new();

    private readonly Guid gameId = Guid.NewGuid();
    private readonly Guid playerId = Guid.NewGuid();
    private readonly string playerName = "Test Player";
    private readonly List<Recipe> recipes = new()
    {
        new Recipe(Guid.NewGuid(), "Chocolate Cake", new List<RecipeIngredient>()),
        new Recipe(Guid.NewGuid(), "Apple Pie", new List<RecipeIngredient>())
    };

    public GameFactoryGrainTests()
    {        
    }

    [Fact]
    public async Task CreateNewGameAsync_ShouldCreateGame_And_ShouldInitializeState()
    {
        // Arrange
        guidProviderMock.Setup(g => g.NewGuid())
            .Returns(gameId);

        recipeRepositoryMock
            .Setup(r => r.GetAllRecipesAsync())
            .ReturnsAsync(recipes);

        // Add all mock objects to the Silo
        Silo.AddService(guidProviderMock.Object);
        Silo.AddService(recipeRepositoryMock.Object);

        // Add probe to verify interactions with the game grain
        var gameGrainMock = Silo.AddProbe<IGameGrain>(gameId);

        // Act
        var grain = await Silo.CreateGrainAsync<GameFactoryGrain>(Guid.Empty);
        var createNewGameCommand = new CreateNewGameCommand(playerId, playerName, DifficultyLevel.Easy);
        var actualGameId = await grain.CreateNewGameAsync(createNewGameCommand);

        // Assert
        Assert.Equal(gameId, actualGameId);
        gameGrainMock.Verify(
            g => g.InitializeGameStateAsync(It.Is<InitializeGameStateCommand>(cmd =>
                cmd.GameId == actualGameId &&
                cmd.PlayerId == playerId &&
                cmd.RecipeIds.SequenceEqual(recipes.Select(r => r.Id))
            )),
            Times.Once
        );
    }

    /// <summary>
    /// Example of a unit test verifying an event is pushed on to a stream.
    /// </summary>
    /// <returns></returns>
    // [Fact]
    // public async Task StartGame_Should_Send_GameStartedEvent()
    // {
    //     // Arrange
    //     var gameId = Guid.NewGuid();
    //     var command = new StartGameCommand(gameId, "Test Game", DateTime.UtcNow);
    //     var stream = Silo.AddStreamProbe<GameStartedEvent>(
    //         gameId, OrleansConstants.STREAM_NAMESPACE_GAME_EVENTS, OrleansConstants.AZURE_QUEUE_STREAM_PROVIDER);
    //     var grain = await Silo.CreateGrainAsync<GameGrain>(gameId);

    //     // Act
    //     await grain.StartGame(command);

    //     // Assert
    //     stream.VerifySend(evt => evt.GameId == gameId, Times.Once());
    // }
}
