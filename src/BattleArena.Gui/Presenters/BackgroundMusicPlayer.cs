using System;
using System.Diagnostics;
using System.IO;
using System.Media;

namespace BattleArena.Gui.Presenters;

internal sealed class BackgroundMusicPlayer : IDisposable
{
    private readonly string _wavPath;
    private SoundPlayer? _player;

    public BackgroundMusicPlayer(string soundsAssetsDir)
    {
        var bgDir = Path.Combine(soundsAssetsDir, "Background");
        _wavPath = Path.Combine(bgDir, "dark-orb-bgsound.wav");

        Directory.CreateDirectory(bgDir);

        if (!File.Exists(_wavPath))
        {
            GenerateAmbientWav(_wavPath);
        }

        var spellCastPath = Path.Combine(soundsAssetsDir, "SpellCast.wav");
        if (!File.Exists(spellCastPath))
        {
            GenerateSpellCastWav(spellCastPath);
        }

        var healCastPath = Path.Combine(soundsAssetsDir, "HealCast.wav");
        if (!File.Exists(healCastPath))
        {
            GenerateHealCastWav(healCastPath);
        }
    }

    public void Play()
    {
        if (!File.Exists(_wavPath))
        {
            Debug.WriteLine("[BGM] WAV not found: " + _wavPath);
            return;
        }

        try
        {
            _player = new SoundPlayer(_wavPath);
            _player.Load();
            _player.PlayLooping();
            Debug.WriteLine("[BGM] Playing: " + _wavPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BGM] Playback failed: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (_player is null)
            return;
        _player.Stop();
        _player.Dispose();
        _player = null;
    }

    public void Dispose() => Stop();

    private static void GenerateAmbientWav(string path)
    {
        var sampleRate = 22050;
        var durationSeconds = 30;
        var totalSamples = sampleRate * durationSeconds;
        var samples = new short[totalSamples];

        for (var i = 0; i < totalSamples; i++)
        {
            var t = (double)i / sampleRate;
            var sample = 0.3 * Math.Sin(2 * Math.PI * 65.4 * t)
                       + 0.2 * Math.Sin(2 * Math.PI * 98.0 * t)
                       + 0.15 * Math.Sin(2 * Math.PI * 130.8 * t);

            var envelope = 1.0;
            if (t < 5) envelope = t / 5;
            else if (t > durationSeconds - 5) envelope = (durationSeconds - t) / 5;
            sample *= envelope * 0.5;

            samples[i] = (short)(sample * short.MaxValue);
        }

        WriteWav(path, samples, sampleRate);
    }

    private static void GenerateSpellCastWav(string path)
    {
        var sampleRate = 22050;
        var durationSeconds = 0.5;
        var totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];

        for (var i = 0; i < totalSamples; i++)
        {
            var t = (double)i / sampleRate;
            var freq = 300 + t * 1200;
            var envelope = t < 0.05 ? t / 0.05 : (t > 0.35 ? 1.0 - (t - 0.35) / 0.15 : 1.0);
            var sample = 0.4 * Math.Sin(2 * Math.PI * freq * t)
                       + 0.15 * Math.Sin(2 * Math.PI * freq * 1.5 * t);
            sample *= envelope;
            samples[i] = (short)(sample * short.MaxValue);
        }

        WriteWav(path, samples, sampleRate);
    }

    private static void GenerateHealCastWav(string path)
    {
        var sampleRate = 22050;
        var durationSeconds = 0.6;
        var totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];

        for (var i = 0; i < totalSamples; i++)
        {
            var t = (double)i / sampleRate;
            var progress = t / durationSeconds;
            var freq = 523 + progress * 400;
            var envelope = progress < 0.1 ? progress / 0.1 : (progress > 0.7 ? 1.0 - (progress - 0.7) / 0.3 : 1.0);
            var sample = 0.35 * Math.Sin(2 * Math.PI * freq * t)
                       + 0.15 * Math.Sin(2 * Math.PI * freq * 2.0 * t);
            sample *= envelope;
            samples[i] = (short)(sample * short.MaxValue);
        }

        WriteWav(path, samples, sampleRate);
    }

    private static void WriteWav(string path, short[] samples, int sampleRate)
    {
        var numChannels = 1;
        var bitsPerSample = 16;
        var byteRate = sampleRate * numChannels * bitsPerSample / 8;
        var blockAlign = (short)(numChannels * bitsPerSample / 8);
        var dataSize = samples.Length * sizeof(short);
        var fileSize = 36 + dataSize;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        bw.Write(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
        bw.Write(fileSize);
        bw.Write(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });

        bw.Write(new[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
        bw.Write(16);
        bw.Write((short)1);
        bw.Write((short)numChannels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write(blockAlign);
        bw.Write((short)bitsPerSample);

        bw.Write(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
        bw.Write(dataSize);
        foreach (var s in samples)
            bw.Write(s);
    }
}
