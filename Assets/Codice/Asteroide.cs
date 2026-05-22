using System.Collections.Generic;
using UnityEngine;

// Rettangolo che fa male se viene toccato.
// Non uso la fisica di Unity, faccio io il controllo "punto dentro rettangolo".
public class Asteroide : MonoBehaviour
{
    // Lista di tutti gli asteroidi presenti nel livello
    public static List<Asteroide> Tutti = new List<Asteroide>();

    // Rettangolo nello spazio del gioco
    public Rect Rettangolo;

    void OnEnable()
    {
        Tutti.Add(this);
    }

    void OnDisable()
    {
        Tutti.Remove(this);
    }

    public void Inizializza(Vector2 centro, Vector2 dimensione)
    {
        transform.position = new Vector3(centro.x, centro.y, 0f);
        // Lo sprite e' 1x1, lo ridimensiono in base a quanto deve essere grande
        transform.localScale = new Vector3(dimensione.x, dimensione.y, 1f);

        Rettangolo = new Rect(
            centro.x - dimensione.x / 2f,
            centro.y - dimensione.y / 2f,
            dimensione.x,
            dimensione.y);
    }

    public bool Contiene(Vector2 punto)
    {
        return Rettangolo.Contains(punto);
    }
}
