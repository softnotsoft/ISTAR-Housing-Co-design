using UnityEngine;
using TMPro;

public class FloorPlanRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float scale = 1f;

    [Header("Parent")]
    public Transform roomsParent;

    public void Render(ApartmentData apartment)
    {
        ClearPreviousRooms();

        foreach (RoomData room in apartment.rooms)
        {
            CreateRoom(room);
        }
    }

    private void CreateRoom(RoomData room)
    {
        GameObject roomObject = new GameObject(room.name);
        roomObject.transform.SetParent(roomsParent);

        // O Unity posiciona objetos pelo centro.
        // O JSON define x e y como o canto inferior esquerdo da divisão.
        // Por isso somamos metade da largura e atura para obter a posição do centro.
        roomObject.transform.position = new Vector3(
            (room.x + room.width / 2f) * scale,
            (room.y + room.height / 2f) * scale,
            0
        );

        SpriteRenderer renderer = roomObject.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateWhiteSprite();
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(room.width * scale, room.height * scale);

        if (ColorUtility.TryParseHtmlString(room.color, out Color parsedColor))
        {
            renderer.color = parsedColor;
        }

        CreateRoomBorder(roomObject.transform, room);

        CreateLabel(roomObject.transform, room);
    }

    private void CreateRoomBorder(Transform parent, RoomData room)
    {
        float width = room.width * scale;
        float height = room.height * scale;
        float thickness = 0.08f;

        CreateBorderLine(parent, "Top Border",
            new Vector3(0, height / 2f, -0.01f),
            new Vector3(width, thickness, 1));

        CreateBorderLine(parent, "Bottom Border",
            new Vector3(0, -height / 2f, -0.01f),
            new Vector3(width, thickness, 1));

        CreateBorderLine(parent, "Left Border",
            new Vector3(-width / 2f, 0, -0.01f),
            new Vector3(thickness, height, 1));

        CreateBorderLine(parent, "Right Border",
            new Vector3(width / 2f, 0, -0.01f),
            new Vector3(thickness, height, 1));
    }

    private void CreateBorderLine(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Quad);
        border.name = name;
        border.transform.SetParent(parent);
        border.transform.localPosition = localPosition;
        border.transform.localScale = localScale;

        Renderer borderRenderer = border.GetComponent<Renderer>();
        borderRenderer.material.color = Color.black;
    }

    private void CreateLabel(Transform parent, RoomData room)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(parent);
        labelObject.transform.localPosition = Vector3.zero;

        TextMeshPro text = labelObject.AddComponent<TextMeshPro>();
        text.text = $"{room.name}\n{room.people} pessoa(s)";
        text.fontSize = 2;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );
    }

    private void ClearPreviousRooms()
    {
        if (roomsParent == null) return;

        foreach (Transform child in roomsParent)
        {
            Destroy(child.gameObject);
        }
    }
}