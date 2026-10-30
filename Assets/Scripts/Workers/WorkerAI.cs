using System;
namespace Brotinho
{
    public static class WorkerAI
    {
        public static long HireCost(int owned) => owned == 0 ? 250 : owned == 1 ? 500 : owned == 2 ? 900 : (long)Math.Round(900 * Math.Pow(1.8, owned - 2));
        public static bool Hire(GameSession game)
        {
            if (game.State.workers.Count >= LevelSystem.WorkerSlots(game.State)) return game.Fail("worker_limit");
            if (!Wallet.Spend(game.State, HireCost(game.State.workers.Count))) return game.Fail("not_enough_coins");
            game.State.workers.Add(new WorkerInstance { definition = game.Data.workers[game.State.workers.Count % game.Data.workers.Length].id });
            game.Bus.Play(SfxEvent.Buy); game.Bus.Refresh(); return true;
        }
        public static bool SetZone(WorkerInstance worker, int x, int y, int width, int height)
        {
            if (x < 0 || y < 0 || width < 1 || height < 1 || x + width > GridManager.WorldSize || y + height > GridManager.WorldSize) return false;
            worker.x = x; worker.y = y; worker.width = width; worker.height = height; return true;
        }
        public static void Tick(GameSession game, WorkerInstance worker)
        {
            if (worker.energy <= 0) worker.resting = true;
            if (worker.task == WorkerTask.Rest || worker.resting)
            {
                worker.energy = Math.Min(100, worker.energy + .12); if (worker.energy >= 35) worker.resting = false; return;
            }
            worker.cooldown = Math.Max(0, worker.cooldown - 1); if (worker.cooldown > 0) return;
            var data = Array.Find(game.Data.workers, x => x.id == worker.definition);
            bool specialty = (int)worker.task - 1 == (int)data.specialty;
            double speed = data.speed * (specialty ? 1.25 : 1) * (worker.energy < 25 ? .5 : 1);
            worker.cooldown = Math.Max(1, Math.Ceiling(4 / speed));
            if (WorkerTaskQueue.Execute(game, worker))
            {
                worker.energy = Math.Max(0, worker.energy - (data.personality == "messy" ? 1.2 : 1));
                worker.history.Add("task_" + worker.task.ToString().ToLowerInvariant()); if (worker.history.Count > 8) worker.history.RemoveAt(0);
            }
            else worker.energy = Math.Min(100, worker.energy + .1);
        }
    }
}
