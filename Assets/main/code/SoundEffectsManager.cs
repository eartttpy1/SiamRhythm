using System.Collections;
using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    // ทำให้เป็น Singleton เพื่อให้เรียกใช้จากสคริปต์อื่นได้ง่าย
    public static SoundEffectsManager instance;

    [SerializeField] private AudioSource soundSFXObject; // ลาก Prefab ที่มี AudioSource มาใส่ตรงนี้
    [SerializeField] private AudioSource soundHitObject;
    private AudioSource musicSource;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ฟังก์ชันสำหรับเล่นเสียง SFX 1 ครั้ง
    public void PlaySoundEffectsClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        // 1. สร้างวัตถุเสียงขึ้นมาในตำแหน่งที่ต้องการ
        AudioSource audioSource = Instantiate(soundSFXObject, spawnTransform.position, Quaternion.identity);

        // 2. กำหนดคลิปเสียงและระดับเสียง
        audioSource.clip = audioClip;
        audioSource.volume = volume;

        // 3. เริ่มเล่นเสียง
        audioSource.Play();

        // 4. หาความยาวของเสียง
        float clipLength = audioSource.clip.length;

        // 5. ทำลายวัตถุเสียงทิ้งเมื่อเล่นจบ เพื่อประหยัด Memory
        Destroy(audioSource.gameObject, clipLength);
    }
    public void PlaySoundHitClip(AudioClip audioClip, Transform spawnTransform, float volume, float pitch = 1f)
    {
        // 1. สร้างวัตถุเสียงขึ้นมาในตำแหน่งที่ต้องการ
        AudioSource audioSource = Instantiate(soundHitObject, spawnTransform.position, Quaternion.identity);

        // 2. กำหนดคลิปเสียงและระดับเสียง
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        // 3. เริ่มเล่นเสียง
        audioSource.Play();

        // 4. หาความยาวของเสียง
        float clipLength = audioSource.clip.length;

        // 5. ทำลายวัตถุเสียงทิ้งเมื่อเล่นจบ เพื่อประหยัด Memory
        // ปรับเวลาทำลายตาม Pitch (ถ้า Pitch ต่ำ เสียงจะยาวขึ้น)
        Destroy(audioSource.gameObject, clipLength / pitch);
    }
    public void PlayBackgroundMusic(AudioClip newClip, float fadeDuration = 1f)
    {
        StartCoroutine(CrossfadeMusic(newClip, fadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        // 1. ถ้ายังไม่มี musicSource ให้สร้างขึ้นมาใหม่
        if (musicSource == null)
        {
            // ใช้ soundSFXObject เป็นต้นแบบ (ซึ่งตั้ง Output เป็น Music Group ไว้แล้ว)
            musicSource = Instantiate(soundSFXObject, Vector3.zero, Quaternion.identity);
            musicSource.transform.SetParent(transform);
            musicSource.loop = true;
        }

        // 2. ถ้าเป็นเพลงเดิมที่เล่นอยู่แล้ว ไม่ต้องทำอะไร
        if (musicSource.clip == newClip) yield break;

        float startVolume = musicSource.volume;

        // 3. Fade Out เพลงเก่า
        if (musicSource.isPlaying)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
                yield return null;
            }
        }

        // 4. เปลี่ยนเพลงและ Fade In เพลงใหม่
        musicSource.clip = newClip;
        musicSource.Play();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, 1f, t / duration); // 1f คือดังเต็มที่ของ Source แล้วไปคุมที่ Mixer ต่อ
            yield return null;
        }
    }
    
}
