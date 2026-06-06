using System;
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

        var mp3Path = Path.Combine(bgDir, "dark-orb-bgsound.mp3");

        if (!File.Exists(_wavPath) && File.Exists(mp3Path))
        {
            GenerateAmbientWav(_wavPath);
        }
    }

    public void Play()
    {
        if (!File.Exists(_wavPath))
            return;

        _player = new SoundPlayer(_wavPath);
        _player.Load();
        _player.PlayLooping();
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
