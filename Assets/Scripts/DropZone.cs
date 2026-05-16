using UnityEngine;

// Cestino: contiene la categoria che accetta.
// La validazione vera (giusto/sbagliato) avviene in Draggable.OnMouseUp,
// confrontando la stringa "category" dell'oggetto con "acceptedCategory" della zona.
public class DropZone : MonoBehaviour
{
    public string acceptedCategory;
}
