using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
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

            // Statystyki
            player.MaxHealth = 6000f;
            player.Health = 6000f;
            player.HumeShield = 500f;

            // TRICK NA UKRYCIE ZOMBIAKA DLA INNYCH
            player.EnableEffect(EffectType.Invisible, 253);

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
            player.DisableEffect(EffectType.Invisible);
            player.DisableEffect(EffectType.Slowness);
            player.Scale = Vector3.one;

            if (player.GameObject.TryGetComponent(out Scp153Component comp))
                Object.Destroy(comp);

            base.RoleRemoved(player);
        }
    }
}