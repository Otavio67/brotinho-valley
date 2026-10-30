using System;
namespace Brotinho
{
    public static class SaveStateValidator
    {
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
        public static bool Basic(GameState s)
        {
            // Existing version-one saves predate cosmetics; keep their first avatar unlocked.
            if (s.unlockedAvatars == null) s.unlockedAvatars = new System.Collections.Generic.List<int> { 0 };
            if (s.unlockedAvatars.Count == 0 || !s.unlockedAvatars.Contains(s.avatar) || s.unlockedAvatars.Exists(a => a < 0 || a > 5)) return false;
            if (s.region == null || s.playerName == null || !Finite(s.elapsed) || !Finite(s.tickRemainder) || s.tickRemainder >= 1 || s.savedUtc < 0 || s.entitySequence < 0 || s.orderSerial < 0 || s.tutorial < 0 || s.tutorial > 6) return false;
            foreach (var item in s.inventory) if (item == null || string.IsNullOrEmpty(item.id) || item.count < 0) return false;
            foreach (var crop in s.crops) if (crop == null || string.IsNullOrEmpty(crop.id) || string.IsNullOrEmpty(crop.definition) || crop.x < 0 || crop.y < 0 || crop.x >= 21 || crop.y >= 21 || !Finite(crop.growth) || !Finite(crop.wateredUntil)) return false;
            foreach (var building in s.buildings) if (building == null || string.IsNullOrEmpty(building.id) || string.IsNullOrEmpty(building.definition) || building.x < 0 || building.y < 0 || building.x >= 21 || building.y >= 21 || building.queue < 0 || building.queue > 5 || building.output < 0 || !Finite(building.progress)) return false;
            foreach (var animal in s.animals) if (animal == null || string.IsNullOrEmpty(animal.definition) || animal.feed < 0 || animal.feed > 10 || animal.ready < 0 || !Finite(animal.progress)) return false;
            foreach (var worker in s.workers) if (worker == null || string.IsNullOrEmpty(worker.definition) || worker.history == null || !Finite(worker.energy) || worker.energy > 100 || !Finite(worker.cooldown) || !Enum.IsDefined(typeof(WorkerTask), worker.task) || worker.width <= 0 || worker.height <= 0 || worker.x < 0 || worker.y < 0 || worker.x + worker.width > 21 || worker.y + worker.height > 21) return false;
            foreach (var block in s.land) if (block == null || block.x < 0 || block.y < 0 || block.x + 4 > 21 || block.y + 4 > 21) return false;
            foreach (var order in s.orders) if (order == null || order.quantity <= 0 || order.reward < 0 || order.gems < 0 || !Finite(order.deadline)) return false;
            return true;
        }
        public static bool ForDatabase(GameState s, GameDatabase db)
        {
            if (s.region != "" && db.Region(s.region) == null) return false;
            foreach (var c in s.crops) if (db.Crop(c.definition) == null) return false;
            foreach (var b in s.buildings) if (db.Building(b.definition) == null) return false;
            foreach (var a in s.animals) if (db.Animal(a.definition) == null || !s.buildings.Exists(b => b.id == a.building && b.definition == db.Animal(a.definition).housing)) return false;
            foreach (var w in s.workers) if (!Array.Exists(db.workers, d => d.id == w.definition) || db.Crop(w.crop) == null) return false;
            return true;
        }
    }
}
