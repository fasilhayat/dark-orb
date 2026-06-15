namespace BattleArena.Gui.Presenters;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using BattleArena.Application.Interfaces;

internal sealed class AvaloniaSoundPlayer : ISoundPlayer
{
    private readonly Dictionary<string, SoundPlayer> _players = new();
    private readonly string _soundsDir;

    public AvaloniaSoundPlayer(string soundsDir)
    {
        _soundsDir = soundsDir;
    }

    public void Play(string soundId)
    {
        if (!OperatingSystem.IsWindows())
        {
            Debug.WriteLine($"[Sound] Playback skipped (non-Windows): {soundId}");
            return;
        }

        if (string.IsNullOrEmpty(soundId)) return;

        if (!_players.TryGetValue(soundId, out var player))
        {
            var path = Path.Combine(_soundsDir, $"{soundId}.wav");
            if (!File.Exists(path))
            {
                Debug.WriteLine($"[Sound] WAV not found: {path}");
                _players[soundId] = null!;
                return;
            }
            player = new SoundPlayer(path);
            player.Load();
            _players[soundId] = player;
        }

        if (player is null) return;

        Task.Run(() =>
        {
            try { player.PlaySync(); }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Sound] Playback failed for '{soundId}': {ex.Message}");
            }
        });
    }
}