namespace Brotinho
{
    public static class GemShop
    {
        // Gems are earned in play. There is no purchase provider or paid offline-report skip.
        public static bool UnlockAvatar(GameSession game, int avatar)
        {
            if (avatar < 0 || avatar > 5 || game.State.unlockedAvatars.Contains(avatar)) return false;
            if (game.State.gems < game.Data.avatarGemCost) return game.Fail("not_enough_gems");
            game.State.gems -= game.Data.avatarGemCost; game.State.unlockedAvatars.Add(avatar); game.State.avatar = avatar; game.Bus.Play(SfxEvent.Buy); game.Bus.Refresh(); return true;
        }
        public static bool BoostCrop(GameSession game, string id)
        {
            var crop = game.State.crops.Find(c => c.id == id); if (crop == null || crop.Ready(game.Data.Crop(crop.definition))) return false;
            if (game.State.gems < game.Data.speedupGemCost) return game.Fail("not_enough_gems");
            game.State.gems -= game.Data.speedupGemCost; crop.growth = crop.Duration(game.Data.Crop(crop.definition)); game.Bus.Play(SfxEvent.Buy); game.Bus.Refresh(); return true;
        }
    }
}
