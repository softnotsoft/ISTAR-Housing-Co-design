using UnityEngine;

public class FloorPlanCameraController : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;
    public Transform roomsParent;

    [Header("Settings")]
    public float padding = 1.5f;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    [Header("Pan")]
    public float panSpeed = 1f;
    private Vector3 lastMousePosition;

    private void Update()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll == 0f)
            return;

        targetCamera.orthographicSize -= scroll * zoomSpeed;

        targetCamera.orthographicSize = Mathf.Clamp(
            targetCamera.orthographicSize,
            minZoom,
            maxZoom
        );
    }

    // Permite navegar pela planta através de pan com o botão do meio do rato.
    // A diferença entre a posição atual e anterior do rato é convertida num deslocamento
    // da câmara, permitindo explorar plantas maiores sem alterar os dados da planta.
    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            Vector3 movement = new Vector3(
                -delta.x,
                -delta.y,
                0f
            ) * panSpeed * targetCamera.orthographicSize * Time.deltaTime;

            targetCamera.transform.position += movement;

            lastMousePosition = Input.mousePosition;
        }
    }

    public void FocusOnFloorPlan()
    {
        if (targetCamera == null || roomsParent == null)
        {
            Debug.LogWarning("Camera ou RoomsParent não atribuídos.");
            return;
        }

        if (roomsParent.childCount == 0)
        {
            Debug.LogWarning("Não existem divisões para centrar a câmara.");
            return;
        }

        Bounds bounds = CalculateBounds();

        Vector3 center = bounds.center;

        // Centra a câmara no centro da planta, mantendo a posição z atual para não alterar a distância.
        targetCamera.transform.position = new Vector3(
            center.x,
            center.y,
            targetCamera.transform.position.z
        );

        targetCamera.orthographic = true;

        float verticalSize = bounds.size.y / 2f + padding;
        float horizontalSize = bounds.size.x / (2f * targetCamera.aspect) + padding;

        targetCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    private Bounds CalculateBounds()
    {
        Renderer firstRenderer = roomsParent.GetChild(0).GetComponent<Renderer>();
        Bounds bounds = firstRenderer.bounds;

        foreach (Transform child in roomsParent)
        {
            Renderer renderer = child.GetComponent<Renderer>();

            if (renderer != null)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return bounds;
    }
}