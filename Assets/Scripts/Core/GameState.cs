using System;
using System.Collections.Generic;

namespace Brotinho
{
    [Serializable] public sealed class ItemStack
    {
        public string id;
        public int count;
        public ItemStack(string id, int count) { this.id = id; this.count = count; }
    }
    [Serializable] public sealed class GameState
    {
        public int version = 1;
        public string region = "";
        public long coins = 200;
        public int gems, xp, expansions, tutorial;
        public double elapsed;
        public double tickRemainder;
        public long savedUtc;
        public string playerName = "Brotinho";
        public int avatar;
        public string language = "pt-BR";
        public bool sound = true, music = true, vibration = true, notifications;
        public int playerX = 10, playerY = 10;
        public long harvested, sold, produced, ordersCompleted;
        public int dailyDay = -1;
        public long dailyHarvestBase, dailySalesBase, dailyOrdersBase;
        public List<string> dailyClaimed = new List<string>();
        public List<string> claimedAchievements = new List<string>();
        public List<ItemStack> inventory = new List<ItemStack>();
        public List<CropInstance> crops = new List<CropInstance>();
        public List<BuildingInstance> buildings = new List<BuildingInstance>();
        public List<AnimalInstance> animals = new List<AnimalInstance>();
        public List<WorkerInstance> workers = new List<WorkerInstance>();
        public List<OrderInstance> orders = new List<OrderInstance>();
        public List<LandBlock> land = new List<LandBlock>();
        public List<MarketSale> market = new List<MarketSale>();
        public static GameState New()
        {
            var s = new GameState();
            s.inventory.Add(new ItemStack("seed_wheat", 6));
            s.inventory.Add(new ItemStack("seed_lettuce", 6));
            s.inventory.Add(new ItemStack("seed_carrot", 4));
            s.inventory.Add(new ItemStack("seed_apple", 2));
            return s;
        }
        public int Count(string id) { var stack = inventory.Find(x => x.id == id); return stack == null ? 0 : stack.count; }
        public int Capacity => 50 + buildings.FindAll(x => x.definition == "storage_shed").Count * 50;
        public bool CanAdd(string id, int amount) => amount >= 0 && (long)Count(id) + amount <= Capacity;
        public bool Add(string id, int amount)
        {
            if (!CanAdd(id, amount)) return false;
            var stack = inventory.Find(x => x.id == id);
            if (stack == null) inventory.Add(new ItemStack(id, amount)); else stack.count += amount;
            return true;
        }
        public bool Take(string id, int amount)
        {
            if (amount < 0 || Count(id) < amount) return false;
            inventory.Find(x => x.id == id).count -= amount;
            return true;
        }
    }
    [Serializable] public sealed class LandBlock { public int x, y; public LandBlock(int x, int y) { this.x = x; this.y = y; } }
    [Serializable] public sealed class MarketSale { public string product; public double pressure, lastTime; }
    [Serializable] public sealed class OrderInstance { public string id, villager, product; public int quantity, reward, xp; public int gems; public double deadline; }
    [Serializable] public sealed class OfflineReport
    {
        public double seconds;
        public long harvests, production, sales, coins;
        public List<string> issues = new List<string>();
        public void Issue(string key) { if (!issues.Contains(key)) issues.Add(key); }
    }
}
