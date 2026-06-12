using UnityEngine;
using TMPro;

public class FloorPlanRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float scale = 1f;

    [Header("Parent")]
    public Transform roomsParent;

    [Header("Sprites")]
    public Sprite roomSprite;

    public void Render(BaseApartmentData apartment)
    {
        ClearPreviousRooms();

        CreateApartmentBoundary(apartment);
    }

    private void CreateApartmentBoundary(BaseApartmentData apartment)
    {
        GameObject boundaryObject = new GameObject("Apartment Boundary");
        boundaryObject.transform.SetParent(roomsParent);
    
        for (int i = 0; i < apartment.boundary.Length; i++)
        {
            PointData currentPoint = apartment.boundary[i];
            PointData nextPoint = apartment.boundary[(i + 1) % apartment.boundary.Length];
    
            Vector3 start = new Vector3(currentPoint.x * scale, currentPoint.y * scale, 0);
            Vector3 end = new Vector3(nextPoint.x * scale, nextPoint.y * scale, 0);
    
            CreateLine(boundaryObject.transform, $"Boundary Wall {i + 1}", start, end);
        }
    }

    private void CreateRoom(RoomData room)
    {
        GameObject roomObject = new GameObject(room.name);
        roomObject.transform.SetParent(roomsParent);

        if (room.points == null || room.points.Length < 3)
        {
            Debug.LogWarning($"A divisão {room.name} não tem pontos suficientes para formar um polígono.");
            return;
        }

        CreatePolygonOutline(roomObject.transform, room);
        CreateLabel(roomObject.transform, room);
    }

    private void CreatePolygonOutline(Transform parent, RoomData room)
    {
        for (int i = 0; i < room.points.Length; i++)
        {
            PointData currentPoint = room.points[i];
            PointData nextPoint = room.points[(i + 1) % room.points.Length];

            Vector3 start = new Vector3(currentPoint.x * scale, currentPoint.y * scale, 0);
            Vector3 end = new Vector3(nextPoint.x * scale, nextPoint.y * scale, 0);

            CreateLine(parent, $"Wall {i + 1}", start, end);
        }
    }

    private void CreateLine(Transform parent, string name, Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject(name);
        lineObject.transform.SetParent(parent);

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;

        lineRenderer.useWorldSpace = true;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = Color.black;
        lineRenderer.material = lineMaterial;
    }

    private void CreateLabel(Transform parent, RoomData room)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(parent);
        labelObject.transform.position = CalculatePolygonCenter(room);

        TextMeshPro text = labelObject.AddComponent<TextMeshPro>();
        text.text = $"{room.name}\n{room.people} pessoa(s)";
        text.fontSize = 2;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
    }

    private void ClearPreviousRooms()
    {
        if (roomsParent == null) return;

        foreach (Transform child in roomsParent)
        {
            Destroy(child.gameObject);
        }
    }

    private Vector3 CalculatePolygonCenter(RoomData room)
    {
        Vector3 sum = Vector3.zero;

        foreach (PointData point in room.points)
        {
            sum += new Vector3(point.x * scale, point.y * scale, 0);
        }

        return sum / room.points.Length;
    }
}