using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MEC;
using PlayerRoles;

namespace SCP153.Logic
{
    public static class LogicHandler
    {
        private static readonly Dictionary<Player, float> EatCooldowns = new Dictionary<Player, float>();

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            if (ev.Player != null && EatCooldowns.ContainsKey(ev.Player))
                EatCooldowns.Remove(ev.Player);
        }

        private static void OnRoundStarted()
        {
            EatCooldowns.Clear();

            // Czekamy 5s - dłużej niż SCP-066 (który czeka 3s)
            // żeby mieć pewność że 066 już się przypisał zanim 153 wybiera pulę
            Timing.CallDelayed(5.0f, () =>
            {
                if (Random.value > Plugin.Instance.Config.SpawnChance) return;

                if (!CustomRole.TryGet(153, out CustomRole bulsonRole)) return;

                // Wykluczamy 079 i graczy którzy już mają jakąkolwiek CustomRole (np. SCP-066)
                var scpPool = Player.List.Where(p => p.Role.Team == Team.SCPs
                                                && p.Role.Type != RoleTypeId.Scp079
                                                && !CustomRole.Registered.Any(r => r.Check(p))).ToList();

                if (scpPool.Count == 0)
                {
                    Log.Warn("[SCP-153] Brak dostepnych SCP do przypisania roli (wszyscy maja juz CustomRole?)");
                    return;
                }

                Player selected = scpPool[Random.Range(0, scpPool.Count)];
                bulsonRole.AddRole(selected);

                // Teleport do Server Room w HCZ
                Timing.CallDelayed(1f, () =>
                {
                    if (selected == null || !selected.IsAlive) return;

                    var room = Room.List.FirstOrDefault(r => r.Name.ToLower().Contains("server") && r.Zone == ZoneType.HeavyContainment);

                    // Fallback na HczArmory
                    if (room == null)
                        room = Room.List.FirstOrDefault(r => r.Type == RoomType.HczArmory);

                    if (room != null) selected.Teleport(room.Position + Vector3.up * 1.5f);
                });

                Log.Info($"[SCP-153] Wylosowano gracza {selected.Nickname} z puli SCP.");
            });
        }

        private static void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null) return;

            if (!CustomRole.TryGet(153, out CustomRole bulsonRole)) return;
            if (!bulsonRole.Check(ev.Attacker)) return;

            float now = Time.time;
            if (EatCooldowns.TryGetValue(ev.Attacker, out float last) && now - last < Plugin.Instance.Config.EatCooldown)
            {
                ev.IsAllowed = false;
                return;
            }

            EatCooldowns[ev.Attacker] = now;
            ev.Amount = Plugin.Instance.Config.Damage;
            ev.Attacker.HumeShield += 100f;

            // Odtwarzamy dźwięk ataku przez komponent
            if (ev.Attacker.GameObject.TryGetComponent(out Scp153Component comp))
                comp.PlayAttackSound();

            Timing.CallDelayed(0.1f, () =>
            {
                if (ev.Player == null || !ev.Player.IsAlive) return;

                var rooms = Room.List.Where(r => r.Zone == ZoneType.LightContainment || r.Zone == ZoneType.HeavyContainment).ToList();
                if (rooms.Any())
                    ev.Player.Position = rooms[Random.Range(0, rooms.Count)].Position + Vector3.up;
            });
        }
    }
}