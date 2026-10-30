using NUnit.Framework;
using UnityEngine;
namespace Brotinho.Tests
{
    public class GemShopTests
    {
        [Test] public void AvatarUnlockSpendsEarnedGemsOnce()
        {
            var game = new GameSession(GameState.New(), Resources.Load<GameDatabase>("GameDatabase")); game.State.gems = 5;
            Assert.That(GemShop.UnlockAvatar(game, 1), Is.True); Assert.That(game.State.gems, Is.Zero); Assert.That(game.State.avatar, Is.EqualTo(1));
            Assert.That(GemShop.UnlockAvatar(game, 1), Is.False); Assert.That(game.State.gems, Is.Zero);
        }
        [Test] public void BoostRequiresGemsAndCannotChargeTwice()
        {
            var game = new GameSession(GameState.New(), Resources.Load<GameDatabase>("GameDatabase")); game.Plant("lettuce", 8, 8); var crop = game.State.crops[0];
            Assert.That(GemShop.BoostCrop(game, crop.id), Is.False); game.State.gems = 2; Assert.That(GemShop.BoostCrop(game, crop.id), Is.True);
            Assert.That(crop.Ready(game.Data.Crop("lettuce")), Is.True); Assert.That(GemShop.BoostCrop(game, crop.id), Is.False); Assert.That(game.State.gems, Is.EqualTo(1));
        }
    }
}
