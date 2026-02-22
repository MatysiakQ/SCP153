using Exiled.API.Features;
using MEC;
using Mirror;
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

        // Do wygładzania pozycji Y
        private float _smoothY;
        private const float YSmoothSpeed = 8f;

        // Offset modelu względem gracza - dostosuj jeśli model nie siedzi idealnie
        private const float OffsetX = 0f;      // lewo / prawo
        private const float OffsetY = -0.95f;  // góra / dół
        private const float OffsetZ = 0.3f;    // przód (dodatnie) / tył (ujemne)

        private void Start()
        {
            _player = Player.Get(gameObject);
            if (_player == null)
            {
                Log.Error("[SCP-153] Player.Get zwrócił null w Start!");
                Destroy(this);
                return;
            }

            if (Plugin.Instance == null)
            {
                Log.Error("[SCP-153] Plugin.Instance jest null!");
                Destroy(this);
                return;
            }

            // --- MODEL ---
            string schemName = Plugin.Instance.Config.SchematicName;

            try
            {
                Log.Info($"[SCP-153] Próba załadowania schematu: {schemName}");
                _schematic = ObjectSpawner.SpawnSchematic(schemName, _player.Position);

                if (_schematic != null)
                {
                    Log.Info("[SCP-153] Schemat załadowany pomyślnie");
                    _schematic.transform.parent = null;
                    _schematic.transform.localScale = Vector3.one;

                    _smoothY = _player.Position.y + OffsetY;

                    Timing.CallDelayed(1.5f, () =>
                    {
                        if (_player != null && _player.Connection != null && _schematic != null)
                        {
                            NetworkIdentity ni = _schematic.gameObject.GetComponent<NetworkIdentity>();
                            if (ni != null)
                                _player.Connection.Send(new ObjectDestroyMessage { netId = ni.netId });
                        }
                    });
                }
                else
                {
                    Log.Error($"[SCP-153] Nie znaleziono schematu: {schemName}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"[SCP-153] BŁĄD podczas ładowania schematu: {e}");
                // Kontynuuj bez modelu
            }

            foreach (var r in _player.GameObject.GetComponentsInChildren<Renderer>())
                _zombieRenderers.Add(r);

            // --- AUDIO ---
            Timing.CallDelayed(1.2f, () =>
            {
                try
                {
                    string filePath = Path.Combine(AudioPath, "HAPS.ogg");

                    if (!File.Exists(filePath))
                    {
                        Log.Warn($"[SCP-153] Brak pliku audio: {filePath}");
                        return;
                    }

                    AudioClipStorage.LoadClip(filePath, "HAPS");

                    string botId = $"SCP153-{_player.Id}-{UnityEngine.Random.Range(1, 9999)}";

                    _audioPlayer = AudioPlayer.CreateOrGet(botId, onIntialCreation: (p) =>
                    {
                        p.transform.parent = _player.GameObject.transform;

                        _speaker = p.AddSpeaker("Main", isSpatial: true, minDistance: 1f, maxDistance: 20f);
                        _speaker.transform.parent = _player.GameObject.transform;
                        _speaker.transform.localPosition = Vector3.zero;
                    });

                    Log.Debug($"[SCP-153] AudioPlayer gotowy dla gracza {_player.Nickname}");
                }
                catch (Exception e)
                {
                    Log.Error($"[SCP-153] Audio Init Error: {e}");
                }
            });
        }


        public void PlayAttackSound()
        {
            if (_audioPlayer == null) return;
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
            if (_player == null || !_player.IsAlive || _schematic == null) return;

            Vector3 playerPos = _player.Position;

            // Wygładzanie Y
            _smoothY = Mathf.Lerp(_smoothY, playerPos.y + OffsetY, Time.deltaTime * YSmoothSpeed);


            float yaw = _player.Rotation.eulerAngles.y;
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

            Vector3 localOffset = new Vector3(OffsetX, 0f, OffsetZ);
            Vector3 worldOffset = yawRotation * localOffset;

            _schematic.transform.position = new Vector3(
                playerPos.x + worldOffset.x,
                _smoothY,
                playerPos.z + worldOffset.z
            );

            _schematic.transform.rotation = yawRotation;
        }

        private void OnDestroy()
        {
            if (_schematic != null) _schematic.Destroy();
            if (_audioPlayer != null) _audioPlayer.Destroy();
            _zombieRenderers.Clear();
        }
    }
}