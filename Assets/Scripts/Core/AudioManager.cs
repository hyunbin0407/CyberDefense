using UnityEngine;

namespace CyberDefense.Core
{
    /// <summary>
    /// 프로시저럴로 합성한 효과음(SFX)을 관리하고 재생하는 싱글톤입니다.
    /// 외부 사운드 파일 없이 ProceduralAudioGenerator로 만든 AudioClip을 Awake에서 한 번만
    /// 생성해 캐싱해두고 재사용합니다. 배경음악(BGM)은 다루지 않습니다.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource audioSource;

        private AudioClip towerShootClip;
        private AudioClip enemyDeathClip;
        private AudioClip towerBuildClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // 매번 새로 합성하면 낭비이므로, 시작할 때 한 번만 만들어서 캐싱해둡니다.
            towerShootClip = ProceduralAudioGenerator.GenerateTowerShootSound();
            enemyDeathClip = ProceduralAudioGenerator.GenerateEnemyDeathSound();
            towerBuildClip = ProceduralAudioGenerator.GenerateTowerBuildSound();
        }

        /// <summary>타워가 공격(발사)할 때의 효과음을 재생합니다.</summary>
        public void PlayTowerShootSFX() => PlaySFX(towerShootClip);

        /// <summary>적이 사망했을 때의 효과음을 재생합니다.</summary>
        public void PlayEnemyDeathSFX() => PlaySFX(enemyDeathClip);

        /// <summary>타워를 건설했을 때의 효과음을 재생합니다.</summary>
        public void PlayTowerBuildSFX() => PlaySFX(towerBuildClip);

        /// <summary>
        /// PlayOneShot으로 재생합니다. 이전 효과음을 끊지 않고 겹쳐서 재생할 수 있습니다.
        /// </summary>
        private void PlaySFX(AudioClip clip)
        {
            if (clip == null || audioSource == null) return;
            audioSource.PlayOneShot(clip);
        }

        /// <summary>효과음 전체 볼륨을 0~1 사이 값으로 설정합니다.</summary>
        public void SetSFXVolume(float volume)
        {
            if (audioSource == null) return;
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}
