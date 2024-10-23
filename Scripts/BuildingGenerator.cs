using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    [Header("Dimensions du bâtiment")]
    public float buildingWidth = 10f;
    public float buildingLength = 20f;
    public float buildingHeight = 30f;
    public int numberOfFloors = 3;

    [Header("\nMaterials")]
    public Material matBrick;
    public Material matDoor;
    public Material matWindow;
    public Material matFloor;

    [Header("\nTaille des briques")]
    public Vector3 brickSize = new Vector3(1f, 0.5f, 0.25f);
    public GameObject brickPrefab;

    [Header("\nPorte")]
    public Vector3 doorSize = new Vector3(2f, 3f, 0.1f);
    public GameObject doorPrefab;

    [Header("\nFenêtres")]
    public Vector3 windowSize = new Vector3(2f, 2f, 0.1f); // Taille de la fenêtre
    public GameObject windowPrefab;
    public int windowsPerFloor = 2; // Nombre de fenêtres par étage (mur avant/arrière)

    // Sauvegarde prefab
    public string prefabSavePath = "Assets/GeneratedPrefabs/";

    public GameObject building;

    // Génération du bâtiment
    public void GenerateBuilding()
    {
        // Si un bâtiment existe déjà, on le détruit
        if (building != null)
        {
            ClearBuilding();
        }

        building = new GameObject("Building");
        building.transform.position = this.transform.position;

        float floorHeight = buildingHeight / numberOfFloors;

        // Génération des étages
        for (int i = 0; i < numberOfFloors; i++)
        {
            // Générer murs pour chaque étage
            GenerateWalls(i * floorHeight, floorHeight);

            // Générer un plancher pour chaque étage
            GenerateFloor(i * floorHeight, buildingLength, buildingWidth);
        }

        // Génération du toit
        GenerateFloor(numberOfFloors * floorHeight, buildingLength, buildingWidth);

        // Génération de la porte au rez-de-chaussée
        GenerateDoor();
    }

    // Générer les murs
    private void GenerateWalls(float yOffset, float floorHeight)
    {
        // Avant (avec fenêtres)
        GenerateWall(new Vector3(0, yOffset, -buildingLength / 2), new Vector3(buildingWidth, floorHeight, brickSize.z), true, "front", yOffset);

        // Arrière (avec fenêtres)
        GenerateWall(new Vector3(0, yOffset, buildingLength / 2), new Vector3(buildingWidth, floorHeight, brickSize.z), true, "back", yOffset);

        // Gauche (sans fenêtres)
        GenerateWall(new Vector3(-buildingWidth / 2, yOffset, 0), new Vector3(buildingLength, floorHeight, brickSize.z), false, "left", yOffset);

        // Droite (sans fenêtres)
        GenerateWall(new Vector3(buildingWidth / 2, yOffset, 0), new Vector3(buildingLength, floorHeight, brickSize.z), false, "right", yOffset);
    }

    // Générer un mur constitué de briques
    private void GenerateWall(Vector3 position, Vector3 wallSize, bool horizontal, string wallOrientation, float yOffset)
    {
        int rows = Mathf.CeilToInt(wallSize.y / brickSize.y);
        int columns = Mathf.CeilToInt(wallSize.x / brickSize.x);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 brickPosition = position + new Vector3(
                    (horizontal ? -wallSize.x / 2 + j * brickSize.x + brickSize.x / 2 : 0),
                    i * brickSize.y,
                    (horizontal ? 0 : -wallSize.x / 2 + j * brickSize.x)
                );

                // Vérifier si la brique serait dans la zone de la porte
                if (wallOrientation == "front" && IsInDoorZone(brickPosition, yOffset))
                {
                    continue; // On ignore les briques dans la zone de la porte
                }

                // Vérifier si la brique serait dans la zone d'une fenêtre
                if ((wallOrientation == "front" || wallOrientation == "back") && IsInWindowZone(brickPosition, yOffset))
                {
                    continue; // On ignore les briques dans la zone des fenêtres
                }

                if (brickPrefab != null)
                {
                    GameObject brick = Instantiate(brickPrefab, brickPosition, Quaternion.identity);
                    brick.GetComponent<MeshRenderer>().material = matBrick;
                    brick.tag = "Building";
                    brick.transform.localScale = brickSize;

                    // Appliquer la rotation en fonction de la face du mur
                    if (wallOrientation == "left" || wallOrientation == "right")
                    {
                        // Pour les murs gauche et droit, on tourne les briques de 90 degrés
                        brick.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }
                    else
                    {
                        // Pas de rotation supplémentaire pour les murs avant et arrière
                        brick.transform.rotation = Quaternion.identity;
                    }

                    brick.transform.parent = building.transform;
                }
            }
        }

        // Générer les fenêtres (sur le mur avant ou arrière)
        if (wallOrientation == "front" || wallOrientation == "back")
        {
            GenerateWindows(wallOrientation, yOffset);
        }
    }

    // Générer un plancher (ou plafond)
    private void GenerateFloor(float yOffset, float length, float width)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.GetComponent<MeshRenderer>().material = matFloor;
        floor.transform.localScale = new Vector3(width, 0.1f, length);
        floor.transform.position = new Vector3(0, yOffset, 0);
        floor.transform.parent = building.transform;
    }

    // Générer la porte
    private void GenerateDoor()
    {
        if (doorPrefab != null)
        {
            GameObject door = Instantiate(doorPrefab, new Vector3(0, doorSize.y / 2, -buildingLength / 2), Quaternion.identity);
            door.GetComponent<MeshRenderer>().material = matDoor;
            door.transform.localScale = doorSize;
            door.transform.parent = building.transform;
        }
    }

    // Générer les fenêtres
    private void GenerateWindows(string wallOrientation, float yOffset)
    {
        float windowSpacing = buildingWidth / (windowsPerFloor + 1); // Espace entre chaque fenêtre

        for (int i = 1; i <= windowsPerFloor; i++)
        {
            float windowX = -buildingWidth / 2 + i * windowSpacing;
            float windowY = yOffset + windowSize.y;

            Vector3 windowPosition = new Vector3(windowX, windowY, (wallOrientation == "front" ? -buildingLength / 2 + windowSize.z / 2 : buildingLength / 2 - windowSize.z / 2));

            if (windowPrefab != null)
            {
                GameObject window = Instantiate(windowPrefab, windowPosition, Quaternion.identity);
                window.GetComponent<MeshRenderer>().material = matWindow;

                if (wallOrientation == "front" || wallOrientation == "back")
                {
                    window.transform.rotation = Quaternion.identity;
                }
                else
                {
                    window.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                window.transform.localScale = windowSize;
                window.transform.parent = building.transform;
            }
        }
    }

    // Vérifie si une brique se trouve dans la zone de la porte
    private bool IsInDoorZone(Vector3 brickPosition, float yOffset)
    {
        float doorTop = doorSize.y;
        float doorBottom = yOffset - doorSize.y;
        float doorLeft = -doorSize.x / 2;
        float doorRight = doorSize.x / 2;

        return brickPosition.y >= doorBottom && brickPosition.y <= doorTop && brickPosition.x >= doorLeft && brickPosition.x <= doorRight;
    }

    // Vérifie si une brique se trouve dans la zone d'une fenêtre
    private bool IsInWindowZone(Vector3 brickPosition, float yOffset)
    {
        float windowBottom = yOffset + windowSize.y / 2;
        float windowTop = yOffset + windowSize.y;
        float windowLeft = -windowSize.x / 2;
        float windowRight = windowSize.x / 2;

        return brickPosition.y >= windowBottom && brickPosition.y <= windowTop && brickPosition.x >= windowLeft && brickPosition.x <= windowRight;
    }

    // Effacer le bâtiment généré
    public void ClearBuilding()
    {
        if (building != null)
        {
            DestroyImmediate(building);
        }
    }
}

