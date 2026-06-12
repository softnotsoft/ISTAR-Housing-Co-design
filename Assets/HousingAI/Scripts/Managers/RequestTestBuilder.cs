using UnityEngine;

public class RequestTestBuilder : MonoBehaviour
{
    public FloorPlanRequestData CreateTestRequest()
    {
        FloorPlanRequestData request = new FloorPlanRequestData();

        request.apartmentId = "apt_01";
        request.totalResidents = 3;

        request.roomRequirements = new RoomRequirementData[]
        {
            new RoomRequirementData { type = "living_room", people = 3 },
            new RoomRequirementData { type = "kitchen", people = 3 },
            new RoomRequirementData { type = "bedroom", people = 2 },
            new RoomRequirementData { type = "bedroom", people = 1 },
            new RoomRequirementData { type = "bathroom", people = 3 }
        };

        return request;
    }

    private void Start()
    {
        FloorPlanRequestData request = CreateTestRequest();

        string json = JsonUtility.ToJson(request, true);

        Debug.Log("Pedido gerado para IA:");
        Debug.Log(json);
    }
}