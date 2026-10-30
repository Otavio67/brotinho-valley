using System;
namespace Brotinho
{
    public static class ProcessingBuilding
    {
        public const int MaxQueue = 5;
        public static bool Enqueue(GameSession game, BuildingInstance building)
        {
            var data = game.Data.Building(building.definition);
            if (data.processSeconds <= 0 || building.queue >= MaxQueue) return game.Fail("queue_full");
            foreach (var input in data.inputs) if (game.State.Count(input.id) < input.count) return game.Fail("empty_input");
            foreach (var input in data.inputs) game.State.Take(input.id, input.count);
            building.queue++; game.Bus.Refresh(); return true;
        }
        public static void Tick(GameSession game, BuildingInstance building)
        {
            var data = game.Data.Building(building.definition);
            if (data.processSeconds <= 0 || building.queue == 0 || building.output > 0) return;
            bool wheel = game.State.region == "riverside" && game.State.buildings.Exists(x => x.definition == "water_wheel");
            building.progress += wheel ? 1.2 : 1;
            if (building.progress + .000001 < data.processSeconds) return;
            building.progress = 0; building.queue--; building.output = data.outputCount; game.State.produced += data.outputCount;
        }
        public static bool Collect(GameSession game, BuildingInstance building)
        {
            if (building.output <= 0) return false; var data = game.Data.Building(building.definition);
            if (!game.State.Add(data.output, building.output)) return game.Fail("storage_full");
            building.output = 0; game.Bus.Play(SfxEvent.Notification); game.Bus.Refresh(); return true;
        }
    }
}
