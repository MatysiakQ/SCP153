using Exiled.API.Features;
using MEC;
using ProjectMER;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SCP153
{
    public class Scp153Component : MonoBehaviour
    {
        private Player _player;
        private SchematicObject _schematic;
        private List<Renderer> _zombieRenderers = new List<Renderer>();

        private AudioPlayer _audioPlayer;
        private Speaker _speaker;

        private string AudioPath => Path.Combine(Paths.Configs, "Plugins", "scp153", "Audio");

        private float _smoothY;
        private const float YSmoothSpeed = 8f;

        // Offset - Zmieniony OffsetZ na ujemny, żeby model biegał lekko za plecami gracza
        // Dzięki temu nie zasłania mu ekranu, a my nie musimy używać "zakazanych" komend.
        private const float OffsetX = 0f;
        private const float OffsetY = -0.95f;
        private const float OffsetZ = -0.2f;

        private void Start()
        {
            _player = Player.Get(gameObject);
            if (_player == null || Plugin.Instance == null)
            {
                Destroy(this);
                return;
            }

            string schemName = Plugin.Instance.Config.SchematicName;

            try
            {
                _schematic = ObjectSpawner.SpawnSchematic(schemName, _player.Position);

                if (_schematic != null)
                {
                    _schematic.transform.parent = null;
                    _schematic.transform.localScale = Vector3.one;
                    _smoothY = _player.Position.y + OffsetY;
                }
            }
            catch (Exception e)
            {
                Log.Error($"[SCP-153] BŁĄD schematu: {e}");
            }

            foreach (var r in _player.GameObject.GetComponentsInChildren<Renderer>())
                _zombieRenderers.Add(r);

            Timing.CallDelayed(1.2f, () =>
            {
                try
                {
                    string filePath = Path.Combine(AudioPath, "HAPS.ogg");
                    if (!File.Exists(filePath)) return;

                    AudioClipStorage.LoadClip(filePath, "HAPS");
                    string botId = $"SCP153-{_player.Id}-{UnityEngine.Random.Range(1, 9999)}";

                    // Tworzymy Bota Audio, ale GO NIE PARENTUJEMY
                    _audioPlayer = AudioPlayer.CreateOrGet(botId, onIntialCreation: (p) =>
                    {
                        _speaker = p.AddSpeaker("Main", isSpatial: true, minDistance: 1f, maxDistance: 20f);
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"[SCP-153] Audio Init Error: {e}");
                }
            });
        }

        public void PlayAttackSound()
        {
            if (_audioPlayer != null)
                _audioPlayer.AddClip("HAPS");
        }

        private void LateUpdate()
        {
            if (_player == null || !_player.IsAlive) return;
            foreach (var r in _zombieRenderers)
            {
                if (r != null && r.enabled) r.enabled = false;
            }
        }

        private void Update()
        {
            if (_player == null || !_player.IsAlive) return;

            Vector3 playerPos = _player.Position;
            _smoothY = Mathf.Lerp(_smoothY, playerPos.y + OffsetY, Time.deltaTime * YSmoothSpeed);

            float yaw = _player.Rotation.eulerAngles.y;
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

            Vector3 localOffset = new Vector3(OffsetX, 0f, OffsetZ);
            Vector3 worldOffset = yawRotation * localOffset;

            // 1. Aktualizacja pozycji schematu (modelu MER)
            if (_schematic != null)
            {
                _schematic.transform.position = new Vector3(
                    playerPos.x + worldOffset.x,
                    _smoothY,
                    playerPos.z + worldOffset.z
                );
                _schematic.transform.rotation = yawRotation;
            }

            // 2. BEZPIECZNA aktualizacja pozycji Bota Audio (zamiast parentowania)
            if (_audioPlayer != null && _speaker != null)
            {
                _audioPlayer.transform.position = playerPos;
                _speaker.transform.position = playerPos;
            }
        }

        private void OnDestroy()
        {
            if (_schematic != null) _schematic.Destroy();
            if (_audioPlayer != null) _audioPlayer.Destroy();
            _zombieRenderers.Clear();
        }
    }
}