using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SCP153
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public string SchematicName { get; set; } = "SCP153";

        [Description("Szansa na pojawienie sie (0.1 = 10%).")]
        public float SpawnChance { get; set; } = 0.1f;

        public float Damage { get; set; } = 50f;
        public float EatCooldown { get; set; } = 3f;


    }
}
