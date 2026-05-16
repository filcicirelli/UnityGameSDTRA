using System.Collections.Generic;
using UnityEngine;

// Barriera-asteroide statica: e' una "no-go zone" rettangolare.
// La SpaceShip ogni frame controlla se e' dentro un Barrier
// (test Axis-Aligned Bounding Box) per applicare la penalita'.
public class Barrier : MonoBehaviour
{
    // Lista globale: SpaceShip la scorre per testare collisione punto-rect.
    public static readonly List<Barrier> All = new List<Barrier>();

    public Rect WorldRect { get; private set; }

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public void Init(Vector2 center, Vector2 size)
    {
        transform.position = new Vector3(center.x, center.y, 0f);
        // Lo sprite della barriera e' un tile 1x1: lo scaliamo alla dimensione.
        transform.localScale = new Vector3(size.x, size.y, 1f);
        WorldRect = new Rect(center.x - size.x / 2f,
                             center.y - size.y / 2f,
                             size.x, size.y);
    }

    public bool Contains(Vector2 point) => WorldRect.Contains(point);
}
