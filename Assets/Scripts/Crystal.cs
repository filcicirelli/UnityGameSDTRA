using System.Collections.Generic;
using UnityEngine;

// Cristallo di "polvere cosmica" che il bambino raccoglie con la navicella.
// - Fluttua dolcemente nella sua posizione iniziale (movimento sinusoidale).
// - Se il raggio traente lo "tocca", viene attratto verso la navicella.
// - Quando arriva abbastanza vicino ad Astro/navicella, viene raccolto.
public class Crystal : MonoBehaviour
{
    // Lista globale dei cristalli ancora in scena: serve a SpaceShip
    // per scoprire chi attrarre senza dover usare Physics.
    public static readonly HashSet<Crystal> Active = new HashSet<Crystal>();

    private Vector3 anchor;       // posizione "di riposo" attorno a cui fluttua
    private float phase;          // sfasamento iniziale del bobbing
    private bool pulling;         // questo frame il cristallo viene attratto?
    private Vector3 pullTarget;
    private float pullSpeed;

    void OnEnable()  { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    public void Init(Vector3 position)
    {
        anchor = position;
        transform.position = position;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    // Chiamato ogni frame da SpaceShip quando il cristallo e' nel raggio.
    public void PullToward(Vector3 target, float speed)
    {
        pulling = true;
        pullTarget = target;
        pullSpeed = speed;
    }

    void Update()
    {
        if (pulling)
        {
            // Trascinamento verso la navicella
            transform.position = Vector3.MoveTowards(
                transform.position, pullTarget, pullSpeed * Time.deltaTime);

            // Vicino abbastanza? Raccolto!
            if (Vector3.Distance(transform.position, pullTarget) < 0.35f)
            {
                if (SpaceGameManager.Instance != null)
                    SpaceGameManager.Instance.OnCrystalCollected();
                Astro.NotifyHappyBounce();
                Destroy(gameObject);
                return;
            }
            pulling = false; // si riarmera' il prossimo frame se ancora nel raggio
        }
        else
        {
            // Movimento di fluttuazione attorno all'ancora
            float t = Time.time * 1.5f + phase;
            transform.position = anchor + new Vector3(
                Mathf.Sin(t) * 0.10f,
                Mathf.Cos(t * 0.8f) * 0.12f,
                0f);
        }

        // Scintillio: rotazione leggera + scala pulsante
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 3f + phase) * 8f);
        float s = 1f + Mathf.Sin(Time.time * 6f + phase) * 0.08f;
        transform.localScale = new Vector3(s, s, 1f);
    }
}
