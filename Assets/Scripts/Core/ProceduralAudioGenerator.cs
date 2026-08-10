using UnityEngine;

namespace CyberDefense.Core
{
    /// <summary>
    /// 외부 사운드 파일(mp3/wav) 없이, 코드로 파형을 계산해서 런타임에 AudioClip을 합성하는 유틸리티입니다.
    /// 사인파/사각파/노이즈/주파수 스윕 같은 기본 파형을 조합하고 볼륨 엔벨로프를 적용해서
    /// 8비트/레트로/사이버 느낌의 효과음을 만듭니다.
    /// </summary>
    public static class ProceduralAudioGenerator
    {
        /// <summary>모든 사운드에 공통으로 쓰는 샘플레이트(Hz). 44100은 CD 음질과 동일한 표준값입니다.</summary>
        private const int SampleRate = 44100;

        // ==================== 기본 빌딩 블록 ====================

        /// <summary>
        /// 사인파(부드럽고 맑은 톤)를 생성합니다.
        /// </summary>
        /// <param name="frequency">주파수(Hz). 값이 클수록 음이 높게 들립니다.</param>
        /// <param name="duration">소리 길이(초).</param>
        /// <param name="amplitude">진폭(0~1 권장). 값이 클수록 소리가 큽니다.</param>
        public static float[] GenerateSineWave(float frequency, float duration, float amplitude = 1f)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)SampleRate; // 현재 샘플의 시간(초)
                samples[i] = amplitude * Mathf.Sin(2f * Mathf.PI * frequency * t);
            }

            return samples;
        }

        /// <summary>
        /// 사각파(딱딱하고 거친, 옛날 게임기 느낌)를 생성합니다.
        /// 사인파의 부호(+/-)만 남기는 방식으로 만듭니다.
        /// </summary>
        /// <param name="frequency">주파수(Hz)</param>
        /// <param name="duration">소리 길이(초)</param>
        /// <param name="amplitude">진폭(0~1 권장)</param>
        public static float[] GenerateSquareWave(float frequency, float duration, float amplitude = 1f)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)SampleRate;
                float sine = Mathf.Sin(2f * Mathf.PI * frequency * t);
                // 부호만 취해서 위(+amplitude)/아래(-amplitude)를 딱딱 끊어지게 오가도록 만듭니다.
                samples[i] = amplitude * Mathf.Sign(sine);
            }

            return samples;
        }

        /// <summary>
        /// 화이트 노이즈(모든 주파수가 뒤섞인 "지지직" 잡음)를 생성합니다.
        /// 폭발음, 타격음처럼 거친 질감이 필요할 때 재료로 사용합니다.
        /// </summary>
        /// <param name="duration">소리 길이(초)</param>
        /// <param name="amplitude">진폭(0~1 권장)</param>
        public static float[] GenerateWhiteNoise(float duration, float amplitude = 1f)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = amplitude * Random.Range(-1f, 1f);
            }

            return samples;
        }

        /// <summary>
        /// 시작 주파수에서 끝 주파수까지 시간에 따라 선형으로 변하는 주파수 스윕(치어프)을 생성합니다.
        /// 레이저 발사음, 상승/하강하는 "삐용~" 효과음의 기본 재료입니다.
        /// </summary>
        /// <param name="startFrequency">시작 주파수(Hz). 소리가 시작되는 음높이입니다.</param>
        /// <param name="endFrequency">끝 주파수(Hz). 소리가 끝날 때의 음높이입니다.</param>
        /// <param name="duration">소리 길이(초)</param>
        /// <param name="amplitude">진폭(0~1 권장)</param>
        public static float[] GenerateFrequencySweep(float startFrequency, float endFrequency, float duration, float amplitude = 1f)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
            var samples = new float[sampleCount];

            // 주파수가 시간에 따라 바뀌므로, 각 샘플의 위상(phase)을 그때그때의 주파수만큼씩
            // 누적해서 더해가야 소리가 딱딱 끊기지 않고 자연스럽게 이어집니다.
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)SampleRate;
                float progress = duration > 0f ? t / duration : 0f;
                float currentFrequency = Mathf.Lerp(startFrequency, endFrequency, progress);

                phase += currentFrequency / SampleRate;
                samples[i] = amplitude * Mathf.Sin(2f * Mathf.PI * phase);
            }

            return samples;
        }

        // ==================== 볼륨 엔벨로프 ====================

        /// <summary>
        /// 선형 감쇠(디케이) 엔벨로프를 적용합니다. 처음엔 원래 크기 그대로 시작해서
        /// 끝에 다다를수록 점점 작아져 0이 됩니다. "삐빅", "펑" 류 효과음의 기본 형태입니다.
        /// </summary>
        public static void ApplyLinearDecay(float[] samples)
        {
            int count = samples.Length;
            if (count <= 1) return;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1); // 0(시작) ~ 1(끝)
                samples[i] *= 1f - t;
            }
        }

        /// <summary>
        /// Attack(빠르게 커짐) → Decay(서서히 작아짐) 형태의 엔벨로프를 적용합니다.
        /// </summary>
        /// <param name="samples">엔벨로프를 적용할 샘플 배열</param>
        /// <param name="attackRatio">
        /// 전체 길이 중 소리가 커지는 데 걸리는 비율(0~1).
        /// 예: 0.1이면 앞의 10% 구간 동안 0에서 최대 크기로 커지고, 나머지 90% 구간 동안 서서히 0으로 줄어듭니다.
        /// </param>
        public static void ApplyAttackDecayEnvelope(float[] samples, float attackRatio = 0.1f)
        {
            int count = samples.Length;
            if (count <= 1) return;

            int attackSamples = Mathf.Clamp(Mathf.RoundToInt(count * attackRatio), 1, count);

            for (int i = 0; i < count; i++)
            {
                float envelope;
                if (i < attackSamples)
                {
                    envelope = i / (float)attackSamples; // 0 -> 1 (커지는 구간)
                }
                else
                {
                    int decayLength = Mathf.Max(1, count - attackSamples);
                    float decayT = (i - attackSamples) / (float)decayLength;
                    envelope = 1f - decayT; // 1 -> 0 (줄어드는 구간)
                }

                samples[i] *= envelope;
            }
        }

        /// <summary>
        /// 두 파형을 같은 비율로 섞습니다(샘플별로 더한 뒤 절반으로 나눔).
        /// 길이가 다르면 더 짧은 쪽 길이에 맞춰서 자릅니다.
        /// </summary>
        public static float[] Mix(float[] a, float[] b)
        {
            int count = Mathf.Min(a.Length, b.Length);
            var result = new float[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = (a[i] + b[i]) * 0.5f;
            }

            return result;
        }

        /// <summary>
        /// 샘플 배열로부터 실제로 재생 가능한 AudioClip을 만듭니다. (44100Hz, mono)
        /// </summary>
        private static AudioClip CreateClip(string clipName, float[] samples)
        {
            var clip = AudioClip.Create(clipName, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // ==================== 완성된 효과음 ====================

        /// <summary>
        /// 타워 발사음: 높은 주파수에서 살짝 낮아지는 스윕 + 빠른 감쇠.
        /// 짧고 날카로운 "삐빅" 레이저 발사 느낌입니다.
        /// </summary>
        public static AudioClip GenerateTowerShootSound()
        {
            const float duration = 0.12f; // 0.1~0.15초 사이
            var samples = GenerateFrequencySweep(1800f, 900f, duration, 0.6f);
            ApplyLinearDecay(samples);
            return CreateClip("TowerShootSFX", samples);
        }

        /// <summary>
        /// 적 사망음: 화이트 노이즈(지지직) + 낮은 사각파를 섞은 짧은 "펑" 느낌 + 빠른 감쇠.
        /// </summary>
        public static AudioClip GenerateEnemyDeathSound()
        {
            const float duration = 0.25f; // 0.2~0.3초 사이

            var noise = GenerateWhiteNoise(duration, 0.5f);
            var lowSquare = GenerateSquareWave(90f, duration, 0.5f);
            var samples = Mix(noise, lowSquare);

            ApplyLinearDecay(samples);
            return CreateClip("EnemyDeathSFX", samples);
        }

        /// <summary>
        /// 타워 건설 확인음: 낮은 곳에서 높은 곳으로 올라가는 스윕 두 음을 짧게 연속으로 재생.
        /// "삐빅-삐빅" 하고 확인해주는 느낌입니다.
        /// </summary>
        public static AudioClip GenerateTowerBuildSound()
        {
            const float noteDuration = 0.1f; // 음 하나의 길이 (두 개 합쳐서 약 0.2초)

            var note1 = GenerateFrequencySweep(500f, 700f, noteDuration, 0.5f);
            ApplyAttackDecayEnvelope(note1, 0.15f);

            var note2 = GenerateFrequencySweep(700f, 1000f, noteDuration, 0.5f);
            ApplyAttackDecayEnvelope(note2, 0.15f);

            var samples = new float[note1.Length + note2.Length];
            note1.CopyTo(samples, 0);
            note2.CopyTo(samples, note1.Length);

            return CreateClip("TowerBuildSFX", samples);
        }
    }
}
