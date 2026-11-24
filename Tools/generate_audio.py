import os
import wave

import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SFX_DIR = os.path.join(ROOT, "Assets", "Audio", "Sfx")
MUSIC_DIR = os.path.join(ROOT, "Assets", "Audio", "Music")
SFX_RATE = 44100
MUSIC_RATE = 32000


def db(value):
    return 10.0 ** (value / 20.0)


def write_wav(path, data, rate):
    data = np.clip(data, -1.0, 1.0)
    pcm = (data * 32767.0).astype("<i2")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(rate)
        f.writeframes(pcm.tobytes())


def t_axis(seconds, rate=SFX_RATE):
    return np.arange(int(seconds * rate)) / rate


def phase_of(freq, rate=SFX_RATE):
    freq = np.broadcast_to(freq, freq.shape if hasattr(freq, "shape") else ())
    return 2.0 * np.pi * np.cumsum(freq) / rate


def sine(freq, n=None, rate=SFX_RATE):
    if np.isscalar(freq):
        freq = np.full(n, float(freq))
    return np.sin(phase_of(freq, rate))


def square(freq, n=None, duty=0.5, rate=SFX_RATE):
    if np.isscalar(freq):
        freq = np.full(n, float(freq))
    ph = (np.cumsum(freq) / rate) % 1.0
    return np.where(ph < duty, 1.0, -1.0)


def saw(freq, n=None, rate=SFX_RATE):
    if np.isscalar(freq):
        freq = np.full(n, float(freq))
    ph = (np.cumsum(freq) / rate) % 1.0
    return 2.0 * ph - 1.0


def triangle(freq, n=None, rate=SFX_RATE):
    if np.isscalar(freq):
        freq = np.full(n, float(freq))
    ph = (np.cumsum(freq) / rate) % 1.0
    return 4.0 * np.abs(ph - 0.5) - 1.0


def noise(n, rng):
    return rng.uniform(-1.0, 1.0, n)


def one_pole_lowpass(x, cutoff, rate=SFX_RATE):
    cutoff = np.broadcast_to(np.asarray(cutoff, dtype=float), x.shape)
    a = 1.0 - np.exp(-2.0 * np.pi * cutoff / rate)
    y = np.empty_like(x)
    acc = 0.0
    for i in range(len(x)):
        acc += a[i] * (x[i] - acc)
        y[i] = acc
    return y


def circular_lowpass(x, cutoff, rate):
    twice = one_pole_lowpass(np.concatenate([x, x]), cutoff, rate)
    return twice[len(x):]


def highpass(x, cutoff, rate=SFX_RATE):
    return x - one_pole_lowpass(x, cutoff, rate)


def env_ad(n, attack, rate=SFX_RATE, curve=4.0):
    a = max(1, int(attack * rate))
    e = np.ones(n)
    e[:a] = np.linspace(0.0, 1.0, a)
    tail = np.linspace(1.0, 0.0, n - a) if n > a else np.zeros(0)
    e[a:] = tail ** curve
    return e


def fade_edges(x, fade_in=0.002, fade_out=0.008, rate=SFX_RATE):
    x = x.copy()
    i = max(1, int(fade_in * rate))
    o = max(1, int(fade_out * rate))
    x[:i] *= np.linspace(0.0, 1.0, i)
    x[-o:] *= np.linspace(1.0, 0.0, o)
    x[-1] = 0.0
    return x


def finish(x, peak_db):
    x = x - np.mean(x)
    x = highpass(x, 25.0)
    x = fade_edges(x)
    peak = np.max(np.abs(x))
    if peak > 0:
        x = x / peak * db(peak_db)
    return x


def sweep(start, end, n, curve=1.0):
    k = np.linspace(0.0, 1.0, n) ** curve
    return start * (end / start) ** k


def footstep(seed):
    rng = np.random.default_rng(seed)
    n = int(0.085 * SFX_RATE)
    body = one_pole_lowpass(noise(n, rng), 900 + 250 * (seed % 3))
    thump = sine(sweep(140 + 15 * (seed % 3), 70, n), n) * 0.8
    x = (body * 1.6 + thump) * env_ad(n, 0.002, curve=3.0)
    return finish(x, -14.0)


def player_shoot(seed):
    rng = np.random.default_rng(seed)
    n = int(0.15 * SFX_RATE)
    f = sweep(1500 + 120 * seed, 420, n, 0.6)
    tone = square(f, n, duty=0.3) * 0.5 + sine(f * 1.5, n) * 0.4
    hiss = highpass(noise(n, rng), 3000) * 0.25
    x = (tone + hiss) * env_ad(n, 0.003, curve=2.2)
    return finish(one_pole_lowpass(x, 6000), -10.0)


def enemy_hit(seed):
    rng = np.random.default_rng(seed)
    n = int(0.12 * SFX_RATE)
    crack = noise(n, rng) * env_ad(n, 0.001, curve=8.0)
    knock = sine(sweep(260 + 30 * seed, 95, n, 0.5), n) * env_ad(n, 0.002, curve=3.0)
    x = one_pole_lowpass(crack, 4500) * 0.9 + knock
    return finish(x, -9.0)


def enemy_death():
    rng = np.random.default_rng(11)
    n = int(0.5 * SFX_RATE)
    f = sweep(420, 60, n, 0.7)
    tone = square(f, n, duty=0.4) * 0.45
    crunch = one_pole_lowpass(noise(n, rng), sweep(5000, 400, n)) * 0.9
    x = (tone + crunch) * env_ad(n, 0.004, curve=2.0)
    steps = 12.0
    x = np.round(x * steps) / steps
    return finish(one_pole_lowpass(x, 5000), -8.0)


def player_hurt():
    n = int(0.28 * SFX_RATE)
    t = t_axis(0.28)
    f = sweep(330, 140, n, 0.8)
    tone = square(f, n, duty=0.5) * (0.7 + 0.3 * np.sin(2 * np.pi * 28 * t))
    x = one_pole_lowpass(tone, 2600) * env_ad(n, 0.003, curve=1.6)
    return finish(x, -8.0)


def coin(seed):
    notes = [(1567.98, 2349.32), (1760.0, 2637.02)][seed]
    n = int(0.26 * SFX_RATE)
    split = int(0.06 * SFX_RATE)
    f = np.where(np.arange(n) < split, notes[0], notes[1])
    tone = triangle(f, n) * 0.7 + sine(f * 2.0, n) * 0.25
    x = tone * env_ad(n, 0.002, curve=2.5)
    return finish(x, -11.0)


def patroller_windup():
    rng = np.random.default_rng(21)
    n = int(0.5 * SFX_RATE)
    t = t_axis(0.5)
    f = sweep(70, 150, n, 1.3) * (1 + 0.05 * np.sin(2 * np.pi * 18 * t))
    growl = saw(f, n) * 0.8 + square(f * 0.5, n) * 0.3
    rasp = one_pole_lowpass(noise(n, rng), 700) * 0.6
    swell = np.linspace(0.25, 1.0, n) ** 1.5
    x = one_pole_lowpass(growl + rasp, sweep(500, 2200, n)) * swell
    return finish(x, -9.0)


def patroller_slam():
    rng = np.random.default_rng(22)
    n = int(0.34 * SFX_RATE)
    boom = sine(sweep(110, 38, n, 0.5), n) * env_ad(n, 0.002, curve=2.0)
    debris = one_pole_lowpass(noise(n, rng), 1800) * env_ad(n, 0.001, curve=5.0)
    x = boom * 1.2 + debris * 0.8
    return finish(x, -6.0)


def shooter_aim():
    n = int(0.66 * SFX_RATE)
    t = t_axis(0.66)
    f = sweep(480, 1500, n, 1.4)
    trem = 0.6 + 0.4 * np.sin(2 * np.pi * sweep(6, 30, n) * t)
    whine = (sine(f, n) * 0.7 + triangle(f * 2.0, n) * 0.2) * trem
    swell = np.linspace(0.35, 1.0, n)
    x = whine * swell
    tick_at = int(0.43 * SFX_RATE)
    tick_n = int(0.03 * SFX_RATE)
    tick = square(2400.0, tick_n) * env_ad(tick_n, 0.001, curve=3.0) * 0.6
    x[tick_at:tick_at + tick_n] += tick
    return finish(x, -11.0)


def shooter_fire():
    rng = np.random.default_rng(31)
    n = int(0.2 * SFX_RATE)
    f = sweep(1900, 280, n, 0.5)
    zap = square(f, n, duty=0.25) * 0.6
    sizzle = highpass(noise(n, rng), 2500) * 0.35
    x = (zap + sizzle) * env_ad(n, 0.002, curve=2.4)
    return finish(one_pole_lowpass(x, 7000), -9.0)


def charger_windup():
    rng = np.random.default_rng(41)
    n = int(0.62 * SFX_RATE)
    rumble = saw(sweep(48, 115, n, 1.2), n) * np.linspace(0.3, 1.0, n)
    rumble = one_pole_lowpass(rumble, 900)
    snort = np.zeros(n)
    for start, length in ((0.0, 0.12), (0.22, 0.12)):
        s = int(start * SFX_RATE)
        k = int(length * SFX_RATE)
        burst = one_pole_lowpass(noise(k, rng), 1600) * env_ad(k, 0.01, curve=2.0)
        snort[s:s + k] += burst * 1.4
    x = rumble + snort
    return finish(x, -8.0)


def charger_dash():
    rng = np.random.default_rng(42)
    n = int(0.36 * SFX_RATE)
    air = noise(n, rng)
    band = one_pole_lowpass(air, sweep(600, 4200, n, 0.6)) - one_pole_lowpass(air, 300)
    shape = np.sin(np.linspace(0, np.pi, n)) ** 1.5
    stomp = sine(sweep(90, 50, n), n) * env_ad(n, 0.002, curve=4.0) * 0.8
    x = band * 1.6 * shape + stomp
    return finish(x, -8.0)


def chaser_bite():
    rng = np.random.default_rng(51)
    n = int(0.16 * SFX_RATE)
    x = np.zeros(n)
    for start in (0.0, 0.07):
        s = int(start * SFX_RATE)
        k = int(0.07 * SFX_RATE)
        chomp = square(sweep(520, 180, k), k, duty=0.35) * 0.6 + one_pole_lowpass(noise(k, rng), 2000) * 0.6
        x[s:s + k] += chomp * env_ad(k, 0.002, curve=3.0)
    return finish(one_pole_lowpass(x, 3500), -10.0)


def ui_click():
    n = int(0.05 * SFX_RATE)
    x = (square(1150.0, n, duty=0.5) * 0.5 + sine(2300.0, n) * 0.3) * env_ad(n, 0.001, curve=4.0)
    return finish(one_pole_lowpass(x, 6000), -12.0)


def note_hz(midi):
    return 440.0 * 2.0 ** ((midi - 69) / 12.0)


def jingle(notes, step, hold, peak):
    total = int((len(notes) * step + hold + 0.3) * SFX_RATE)
    x = np.zeros(total)
    for i, midi_notes in enumerate(notes):
        start = int(i * step * SFX_RATE)
        last = i == len(notes) - 1
        length = int(((hold if last else step * 1.6)) * SFX_RATE)
        for j, m in enumerate(midi_notes):
            f = note_hz(m)
            voice = triangle(f, length) * 0.6 if j == 0 else square(f, length, duty=0.25) * 0.18
            x[start:start + length] += voice * env_ad(length, 0.004, curve=1.8)
    x = one_pole_lowpass(x, 7000)
    return finish(x, peak)


def victory():
    notes = [[72, 60], [76, 64], [79, 67], [84, 72], [79, 67], [84, 76]]
    return jingle(notes, 0.13, 0.75, -7.0)


def defeat():
    notes = [[67, 55], [63, 51], [60, 48], [55, 43]]
    return jingle(notes, 0.28, 0.9, -8.0)


class Loop:
    def __init__(self, bpm, bars, rate=MUSIC_RATE):
        self.rate = rate
        self.beat = 60.0 / bpm
        self.length = int(round(bars * 4 * self.beat * rate))
        self.buf = np.zeros(self.length)

    def add(self, start_beat, samples, gain=1.0):
        start = int(round(start_beat * self.beat * self.rate)) % self.length
        idx = (np.arange(len(samples)) + start) % self.length
        np.add.at(self.buf, idx, samples * gain)

    def tone(self, kind, midi, beats, attack=0.005, curve=1.5, duty=0.5, release=None):
        n = int(beats * self.beat * self.rate)
        f = note_hz(midi)
        if kind == "tri":
            s = triangle(f, n, rate=self.rate)
        elif kind == "sq":
            s = square(f, n, duty=duty, rate=self.rate)
        elif kind == "saw":
            s = saw(f, n, rate=self.rate)
        else:
            s = sine(f, n, rate=self.rate)
        return s * env_ad(n, attack, rate=self.rate, curve=curve)

    def pad(self, midi_notes, beats, gain):
        n = int(beats * self.beat * self.rate)
        out = np.zeros(n)
        for m in midi_notes:
            f = note_hz(m)
            out += triangle(f * 1.003, n, rate=self.rate) + triangle(f * 0.997, n, rate=self.rate)
        a = int(0.35 * n)
        r = int(0.35 * n)
        e = np.ones(n)
        e[:a] = np.linspace(0, 1, a)
        e[-r:] = np.linspace(1, 0, r)
        return out * e * gain / max(1, len(midi_notes))

    def drum(self, kind, rng):
        if kind == "kick":
            n = int(0.18 * self.rate)
            return sine(sweep(120, 42, n, 0.4), n, rate=self.rate) * env_ad(n, 0.001, rate=self.rate, curve=2.5)
        if kind == "snare":
            n = int(0.14 * self.rate)
            body = one_pole_lowpass(noise(n, rng), 5000, rate=self.rate)
            return (body * 0.8 + sine(190.0, n, rate=self.rate) * 0.3) * env_ad(n, 0.001, rate=self.rate, curve=3.0)
        n = int(0.04 * self.rate)
        return highpass(noise(n, rng), 6000, rate=self.rate) * env_ad(n, 0.001, rate=self.rate, curve=3.0)

    def echo(self, delay_beats, feedback, taps):
        dry = self.buf.copy()
        shift = int(round(delay_beats * self.beat * self.rate))
        for k in range(1, taps + 1):
            self.buf += np.roll(dry, shift * k) * (feedback ** k)

    def render(self, peak_db):
        x = self.buf - np.mean(self.buf)
        x = np.tanh(x / np.max(np.abs(x)) * 1.2)
        return x / np.max(np.abs(x)) * db(peak_db)


def menu_music():
    loop = Loop(bpm=92, bars=16)
    chords = [
        (57, [57, 60, 64]), (53, [53, 57, 60]), (48, [55, 60, 64]), (55, [55, 59, 62]),
        (57, [57, 60, 64]), (53, [53, 57, 60]), (52, [52, 56, 59]), (52, [52, 56, 59]),
    ]
    arp_pattern = [0, 1, 2, 1, 2, 1, 0, 1]
    melody = [
        [(0, 76, 1.5), (1.5, 74, 0.5), (2, 72, 2)], [(0, 72, 1), (1, 69, 1), (2, 72, 2)],
        [(0, 76, 1.5), (1.5, 79, 0.5), (2, 76, 2)], [(0, 74, 3), (3, 71, 1)],
        [(0, 72, 1.5), (1.5, 74, 0.5), (2, 76, 2)], [(0, 77, 1), (1, 76, 1), (2, 72, 2)],
        [(0, 71, 1.5), (1.5, 72, 0.5), (2, 74, 2)], [(0, 68, 2), (2, 71, 2)],
    ]
    for bar in range(16):
        root, chord = chords[bar % 8]
        b0 = bar * 4
        loop.add(b0, loop.pad([n + 12 for n in chord], 4.2, 0.28))
        loop.add(b0, loop.tone("sine", root - 12, 2.0, attack=0.01, curve=1.2), 0.45)
        loop.add(b0 + 2, loop.tone("sine", root - 12, 2.0, attack=0.01, curve=1.2), 0.38)
        for i, step in enumerate(arp_pattern):
            loop.add(b0 + i * 0.5, loop.tone("tri", chord[step] + 12, 0.9, attack=0.003, curve=3.0), 0.16)
        if bar >= 8:
            for start, midi, length in melody[bar % 8]:
                loop.add(b0 + start, loop.tone("sq", midi, length * 0.95, attack=0.02, curve=1.2, duty=0.25), 0.07)
                loop.add(b0 + start, loop.tone("tri", midi, length * 0.95, attack=0.02, curve=1.2), 0.12)
    loop.buf = circular_lowpass(loop.buf, 5200, loop.rate)
    loop.echo(0.75, 0.32, 3)
    return loop.render(-7.0)


def level_music():
    rng = np.random.default_rng(77)
    loop = Loop(bpm=128, bars=16)
    chords = [(50, [62, 65, 69]), (46, [58, 62, 65]), (48, [60, 64, 67]), (45, [57, 61, 64])]
    bass_rhythm = [0, 0, 12, 0, 0, 12, 0, 7]
    riff = [
        [(0, 74, 0.5), (0.5, 77, 0.5), (1, 81, 1), (2, 79, 0.5), (2.5, 77, 0.5), (3, 76, 1)],
        [(0, 74, 1), (1, 77, 0.5), (1.5, 74, 0.5), (2, 70, 2)],
        [(0, 72, 0.5), (0.5, 76, 0.5), (1, 79, 1), (2, 77, 0.5), (2.5, 76, 0.5), (3, 72, 1)],
        [(0, 73, 1.5), (1.5, 76, 0.5), (2, 81, 1), (3, 79, 1)],
    ]
    kick = loop.drum("kick", rng)
    for bar in range(16):
        root, chord = chords[bar % 4]
        b0 = bar * 4
        for i, offset in enumerate(bass_rhythm):
            loop.add(b0 + i * 0.5, loop.tone("sq", root - 12 + offset, 0.45, attack=0.002, curve=1.8, duty=0.5), 0.12)
            loop.add(b0 + i * 0.5, loop.tone("tri", root - 12 + offset, 0.45, attack=0.002, curve=1.8), 0.22)
        for beat in (0, 2):
            loop.add(b0 + beat, kick, 0.7)
        if bar % 4 == 3:
            loop.add(b0 + 3.5, kick, 0.5)
        for beat in (1, 3):
            loop.add(b0 + beat, loop.drum("snare", rng), 0.32)
        for i in range(8):
            loop.add(b0 + i * 0.5, loop.drum("hat", rng), 0.12 if i % 2 else 0.18)
        for i in range(8):
            loop.add(b0 + i * 0.5 + 0.25, loop.tone("tri", chord[i % 3] + 12, 0.25, attack=0.002, curve=3.0), 0.06)
        if bar >= 4:
            phrase = riff[bar % 4]
            gain = 0.08 if bar < 12 else 0.1
            for start, midi, length in phrase:
                loop.add(b0 + start, loop.tone("sq", midi, length * 0.9, attack=0.004, curve=1.4, duty=0.25), gain)
    loop.buf = circular_lowpass(loop.buf, 7000, loop.rate)
    loop.echo(0.75, 0.22, 2)
    return loop.render(-7.0)


SFX = {
    "footstep_1": lambda: footstep(1),
    "footstep_2": lambda: footstep(2),
    "footstep_3": lambda: footstep(3),
    "player_shoot_1": lambda: player_shoot(0),
    "player_shoot_2": lambda: player_shoot(1),
    "enemy_hit_1": lambda: enemy_hit(0),
    "enemy_hit_2": lambda: enemy_hit(1),
    "enemy_death": enemy_death,
    "player_hurt": player_hurt,
    "coin_1": lambda: coin(0),
    "coin_2": lambda: coin(1),
    "patroller_windup": patroller_windup,
    "patroller_slam": patroller_slam,
    "shooter_aim": shooter_aim,
    "shooter_fire": shooter_fire,
    "charger_windup": charger_windup,
    "charger_dash": charger_dash,
    "chaser_bite": chaser_bite,
    "ui_click": ui_click,
    "victory": victory,
    "defeat": defeat,
}


def main():
    for name, make in SFX.items():
        data = make()
        write_wav(os.path.join(SFX_DIR, name + ".wav"), data, SFX_RATE)
        print(f"{name}.wav {len(data) / SFX_RATE:.2f}s peak {20 * np.log10(np.max(np.abs(data))):.1f} dBFS")
    for name, make in (("music_menu", menu_music), ("music_level", level_music)):
        data = make()
        write_wav(os.path.join(MUSIC_DIR, name + ".wav"), data, MUSIC_RATE)
        edge = abs(data[0] - data[-1])
        print(f"{name}.wav {len(data) / MUSIC_RATE:.2f}s peak {20 * np.log10(np.max(np.abs(data))):.1f} dBFS loop-edge-step {edge:.4f}")


if __name__ == "__main__":
    main()
