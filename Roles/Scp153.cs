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
        public override int MaxHealth { get; set; } = 4500;
        public override float SpawnChance { get; set; } = 0f;

        protected override void RoleAdded(Player player)
        {
            if (player == null || !Check(player)) return;

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

            player.EnableEffect(EffectType.Fade, 255, 9999f);

            player.InfoArea = (PlayerInfoArea)0;

            player.EnableEffect(EffectType.Slowness, 20, 9999f);

            player.Scale = Vector3.one;

            if (!player.GameObject.TryGetComponent(out Scp153Component comp))
                player.GameObject.AddComponent<Scp153Component>();

            base.RoleAdded(player);
        }

        protected override void RoleRemoved(Player player)
        {
            player.DisableEffect(EffectType.Fade);
            player.InfoArea = PlayerInfoArea.Nickname | PlayerInfoArea.CustomInfo | PlayerInfoArea.Badge | PlayerInfoArea.Role;
            player.DisableEffect(EffectType.Slowness);
            player.Scale = Vector3.one;

            if (player.GameObject.TryGetComponent(out Scp153Component comp))
                Object.Destroy(comp);

            base.RoleRemoved(player);
        }
    }
}