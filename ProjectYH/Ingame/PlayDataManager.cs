using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayDataManager : MonoBehaviour
{
    public static PlayDataManager instance;

    public List<DefaultNote> defaultNote;
    public List<DoubleNote> doubleNote;
    public List<LongNote> longNote;

    public List<BaseEffect> baseEffect;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadComplete(LevelData level)
    {
        defaultNote = level.MinuteNotes.OfType<DefaultNote>().ToList();
        doubleNote = level.MinuteNotes.OfType<DoubleNote>().ToList();
        longNote = level.MinuteNotes.OfType<LongNote>().ToList();



        baseEffect = level.Effects;

        if(NoteSpawnManager.instance != null)NoteSpawnManager.instance.LoadComplete();
    }
}
