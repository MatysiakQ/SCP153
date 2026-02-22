using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace SCP153.Roles
{
    public class Scp153 : CustomRole
    {
        public override uint Id { get; set; } = 153;
        public override string Name { get; set; } = "SCP153";
        public override string Description { get; set; } = "Gowniak pozerajacy ofiary";
        public override string CustomInfo { get; set; } = "SCP-153 BULSON";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492; // Rola Zombiaka
        public override int MaxHealth { get; set; } = 6000;
        public override float SpawnChance { get; set; } = 0f;

        protected override void RoleAdded(Player player)
        {
            if (player == null || !Check(player)) return;

            // Delay 0.8f - żeby HpLimiter (który działa na Spawned) nie nadpisał naszego HP
            MEC.Timing.CallDelayed(0.8f, () =>
            {
                if (player == null || !player.IsAlive) return;

                // Statystyki
                player.MaxHealth = 6000f;
                player.Health = 6000f;
                player.HumeShield = 500f;

                // Broadcast dla gracza
                player.Broadcast(10, "<size=30><color=red><b>JESTEŚ SCP-153 BULSON</b></color></size>\n<color=yellow>NIE DAJ IM ZJEŚĆ SWOJEGO KAJMAKU!</color>");
            });

            // UKRYCIE ZOMBIAKA - Fade 255 zamiast Invisible (gracz nie widzi przyciemnionego ekranu)
            player.EnableEffect(EffectType.Fade, 255, 9999f);

            // Ukrycie nicku/ról nad głową (bitmask 0 = nic nie pokazuj)
            player.InfoArea = (PlayerInfoArea)0;

            // Spowolnienie do ~80% prędkości
            player.EnableEffect(EffectType.Slowness, 20, 9999f);

            // Normalny rozmiar gracza
            player.Scale = Vector3.one;

            // Dodajemy komponent, który zajmie się modelem i ukrywaniem ciała
            if (!player.GameObject.TryGetComponent(out Scp153Component comp))
                player.GameObject.AddComponent<Scp153Component>();

            base.RoleAdded(player);
        }

        protected override void RoleRemoved(Player player)
        {
            player.DisableEffect(EffectType.Fade);
            // Przywracamy domyślne info nad głową
            player.InfoArea = PlayerInfoArea.Nickname | PlayerInfoArea.CustomInfo | PlayerInfoArea.Badge | PlayerInfoArea.Role;
            player.DisableEffect(EffectType.Slowness);
            player.Scale = Vector3.one;

            if (player.GameObject.TryGetComponent(out Scp153Component comp))
                Object.Destroy(comp);

            base.RoleRemoved(player);
        }
    }
}