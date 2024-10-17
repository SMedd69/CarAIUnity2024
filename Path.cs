using UnityEngine;

public class Path : MonoBehaviour
{
    public Color rayColor = Color.white;
    public Transform[] path;
    public int curveResolution = 10; // Nombre de segments pour la courbe entre deux points

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

        // Dessiner les courbes entre les points du chemin
        for (int i = 0; i < path.Length; i++)
        {
            Vector3 pos = path[i].position;
            Gizmos.DrawWireSphere(pos, 0.3f);

            // Ne pas dessiner de courbe si on est au premier point
            if (i > 0)
            {
                // Déterminer les points d'interpolation pour la courbe
                Vector3 p0 = path[i - 1].position; // Point précédent
                Vector3 p1 = path[i].position; // Point actuel
                Vector3 p2 = (i < path.Length - 1) ? path[i + 1].position : path[0].position; // Prochain point (ou premier point s'il s'agit du dernier point)
                Vector3 p3 = (i < path.Length - 2) ? path[i + 2].position : path[(i + 1) % path.Length].position; // Point après le prochain (pour Catmull-Rom)

                // Dessiner une courbe entre les points p1 et p2
                DrawCatmullRomCurve(p0, p1, p2, p3);
            }
        }
    }

    void DrawCatmullRomCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 previousPos = p1;

        // Calculer la courbe avec une certaine résolution
        for (int j = 1; j <= curveResolution; j++)
        {
            float t = j / (float)curveResolution;
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);
            Gizmos.DrawLine(previousPos, newPos);
            previousPos = newPos;
        }
    }

    // Fonction qui calcule la position sur la courbe à l'aide de Catmull-Rom
    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Catmull-Rom spline (équation paramétrique)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        return pos;
    }
}
