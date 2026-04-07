using UnityEngine;


public class GameplayControllerSorD : MonoBehaviour
{
    public SongData selectedSong; // ข้อมูลเพลงที่โหลดมา
    public RhythmManager singleManager;
    public DualRhythmManager dualManager;
    [SerializeField] private GameObject singleManagerObject;
    [SerializeField] private GameObject dualManagerObject; 
    void Awake()
    {
        // สมมติว่ารับค่า isDualMode มาจาก Static Variable หรือ PlayerPrefs
        bool isDual = PlayerPrefs.GetInt("IsDualMode", 0) == 1;

        if (isDual)
        {
            singleManagerObject.SetActive(false);
            dualManagerObject.SetActive(true);
            dualManager.LoadSongData(selectedSong);
        }
        else
        {
            dualManagerObject.SetActive(false);
            singleManagerObject.SetActive(true);
            singleManager.LoadSongData(selectedSong);
        }
    }
}
