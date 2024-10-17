using UnityEngine;

public class Path : MonoBehaviour
{
    public Color rayColor = Color.white;
    public Transform[] path;

    void OnDrawGizmos()
    {
        // Changer la couleur des Gizmos
        Gizmos.color = rayColor;

        // Obtenir tous les enfants du GameObject (ce script est sur le parent)
        Transform[] path_objs = GetComponentsInChildren<Transform>();

        // Filtrer pour exclure le transform du parent lui-même
        path = new Transform[path_objs.Length - 1]; // On ne compte pas le parent lui-même
        int index = 0;

        for (int i = 0; i < path_objs.Length; i++)
        {
            if (path_objs[i] != transform) // Ignorer le parent lui-même
            {
                path[index] = path_objs[i];
                index++;
            }
        }

        // Dessiner les lignes entre les points du chemin
        for (int i = 0; i < path.Length; i++)
        {
            Vector3 pos = path[i].position;
            Gizmos.DrawWireSphere(pos, 0.3f);

            // Ne pas dessiner de ligne si on est au premier point
            if (i > 0)
            {
                Vector3 previous = path[i - 1].position;
                Gizmos.DrawLine(previous, pos);
            }

            // Si c'est le dernier point, on boucle vers le premier
            if (i == path.Length - 1)
            {
                Vector3 first = path[0].position;
                Gizmos.DrawLine(path[i].position, first);
            }
        }
    }
}
