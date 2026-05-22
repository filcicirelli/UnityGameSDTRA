using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  Asteroide
// ---------------------------------------------------------------------
//  E' la "barriera": un rettangolo che fa male a chi lo tocca.
//  Non si muove, non ha collider fisici: la collisione e' calcolata
//  a mano da Astro, con un semplice test "punto dentro rettangolo".
//  E' un metodo semplice ma efficace per un gioco 2D senza fisica.
// =====================================================================
public class Asteroide : MonoBehaviour
{
    // Lista globale di tutti gli asteroidi in scena.
    // Astro la legge per testare la collisione del puntatore.
    public static readonly List<Asteroide> Tutti = new List<Asteroide>();

    // Rettangolo nello spazio del mondo
    public Rect Rettangolo { get; private set; }

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
        // Lo sprite e' un tile 1x1: lo scalo per coprire la dimensione voluta.
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
