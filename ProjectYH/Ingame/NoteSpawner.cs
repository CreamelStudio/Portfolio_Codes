using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject defaultNotePrefab;
    public GameObject doubleNotePrefab;
    public GameObject longNotePrefab;
    public GameObject longNoteEndPrefab;

    public GameObject longNoteRendererPrefab;
    public List<LongNoteRenderer> longNoteRenderer;

    public GameObject SpawnDefaultNote()
    {
        return Instantiate(defaultNotePrefab, transform.position, transform.rotation);
    }

    public GameObject SpawnDoubleNote()
    {
        return Instantiate(doubleNotePrefab, transform.position, transform.rotation);
    }

    public GameObject SpawnLongNote(float duration)
    {
        GameObject rendererObj = Instantiate(longNoteRendererPrefab, transform.position, transform.rotation);
        LongNoteRenderer renderer = rendererObj.GetComponent<LongNoteRenderer>();
        renderer.target = transform;
        renderer.StartFollow(duration);
        longNoteRenderer.Add(renderer);

        return Instantiate(longNotePrefab, transform.position, transform.rotation);
    }
    
    public GameObject StopLongNoteRenderer(int val)
    {
        longNoteRenderer[val].StopFollow();
        GameObject endNote = Instantiate(longNoteEndPrefab, transform.position, transform.rotation);
        return endNote;
    }
}
