using UnityEngine;

public class RoomTemplate : MonoBehaviour
{
    [Header("Room Dimensions")]
    [SerializeField] private float width = 10f;
    [SerializeField] private float length = 10f;
    [SerializeField] private float height = 3f;
    private float thickness = 0.1f;

    [Header("Material Settings")]
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material ceilingMaterial;

    private GameObject floor;
    private GameObject ceiling;
    private GameObject[] walls = new GameObject[4];

    [ContextMenu("Generate Room")]
    public void GenerateRoom()
    {
        // Delete existing room elements if they exist
        ClearRoom();

        // Create new room
        CreateFloor();
        CreateCeiling();
        CreateWalls();
    }

    private void ClearRoom()
    {
        // Clear previously generated components
        if (floor != null) DestroyImmediate(floor);
        if (ceiling != null) DestroyImmediate(ceiling);

        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] != null) DestroyImmediate(walls[i]);
        }
    }

    private void CreateFloor()
    {
        floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.parent = this.transform;

        // Position and scale
        floor.transform.localPosition = new Vector3(0, -thickness/2, 0);
        floor.transform.localScale = new Vector3(width, thickness, length);

        // Apply material
        if (floorMaterial != null)
        {
            floor.GetComponent<Renderer>().material = floorMaterial;
        }
    }

    private void CreateCeiling()
    {
        ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.parent = this.transform;

        // Position and scale
        ceiling.transform.localPosition = new Vector3(0, height - thickness/2, 0);
        ceiling.transform.localScale = new Vector3(width, thickness, length);

        // Apply material
        if (ceilingMaterial != null)
        {
            ceiling.GetComponent<Renderer>().material = ceilingMaterial;
        }
    }

    private void CreateWalls()
    {
        // Create four walls

        // North Wall
        walls[0] = CreateWall("North Wall", new Vector3(0, height / 2, length / 2), new Vector3(width, height, thickness));

        // South Wall
        walls[1] = CreateWall("South Wall", new Vector3(0, height / 2, -length / 2), new Vector3(width, height, thickness));

        // East Wall
        walls[2] = CreateWall("East Wall", new Vector3(width / 2, height / 2, 0), new Vector3(thickness, height, length));

        // West Wall
        walls[3] = CreateWall("West Wall", new Vector3(-width / 2, height / 2, 0), new Vector3(thickness, height, length));
    }

    private GameObject CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.parent = this.transform;

        // Position and scale
        wall.transform.localPosition = position;
        wall.transform.localScale = scale;

        // Apply material
        if (wallMaterial != null)
        {
            wall.GetComponent<Renderer>().material = wallMaterial;
        }

        return wall;
    }
}