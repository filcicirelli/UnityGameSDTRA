using System.Collections;
using UnityEngine;

// Oggetto che il bambino puo' trascinare con il mouse.
// Funziona grazie ai messaggi OnMouseDown/Drag/Up di Unity:
// si attivano automaticamente se l'oggetto ha un Collider2D
// e la camera e' taggata "MainCamera".
[RequireComponent(typeof(Collider2D))]
public class Draggable : MonoBehaviour
{
    public string category;          // assegnata in fase di spawn (es. "red", "circle", "frutta")

    private Vector3 startPos;        // posizione iniziale: vi torna se il drop e' sbagliato
    private Camera cam;
    private bool dragging;
    private float zDistance;         // distanza in Z da camera (per ScreenToWorldPoint)

    void Awake()
    {
        cam = Camera.main;
        startPos = transform.position;
        zDistance = Mathf.Abs(cam.transform.position.z - transform.position.z);
    }

    void OnMouseDown()
    {
        dragging = true;
    }

    void OnMouseDrag()
    {
        if (!dragging) return;
        Vector3 m = Input.mousePosition;
        m.z = zDistance;
        Vector3 world = cam.ScreenToWorldPoint(m);
        transform.position = new Vector3(world.x, world.y, startPos.z);
    }

    void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        // Cerca tutti i collider sotto la posizione di rilascio:
        // se uno e' una DropZone, valida il drop.
        var hits = Physics2D.OverlapPointAll(transform.position);
        foreach (var h in hits)
        {
            var zone = h.GetComponent<DropZone>();
            if (zone == null) continue;

            if (zone.acceptedCategory == category)
            {
                GameManager.Instance.OnCorrectDrop();
                Destroy(gameObject);
            }
            else
            {
                GameManager.Instance.OnWrongDrop();
                StartCoroutine(ShakeAndReturn());
            }
            return;
        }

        // Rilasciato fuori da qualsiasi zona: torna in posizione.
        StartCoroutine(ReturnTo(startPos));
    }

    // Effetto "scuoti" + ritorno: feedback visivo di errore.
    IEnumerator ShakeAndReturn()
    {
        Vector3 anchor = transform.position;
        float t = 0f;
        while (t < 0.3f)
        {
            transform.position = anchor + (Vector3)(Random.insideUnitCircle * 0.1f);
            t += Time.deltaTime;
            yield return null;
        }
        yield return ReturnTo(startPos);
    }

    // Movimento morbido verso una posizione (interpolazione lineare).
    IEnumerator ReturnTo(Vector3 target)
    {
        Vector3 from = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            transform.position = Vector3.Lerp(from, target, t);
            yield return null;
        }
        transform.position = target;
    }
}
