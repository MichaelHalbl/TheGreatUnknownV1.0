using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public Room StartRoomPrefab, EndRoomPrefab;
    public List<Room> roomPrefabs = new List<Room>();
    public Vector2 iterationRange = new Vector2(3, 10);

    List<Doorway> availableDoorways = new List<Doorway>();
    Startroom startRoom;
    Endroom endRoom;
    List<Room> placedRooms = new List<Room>();

    LayerMask roomLayerMask;

    void Start()
    {
        roomLayerMask = LayerMask.GetMask("Room");
        StartCoroutine("GenerateLevel");
    }
    IEnumerator GenerateLevel()
    {
        WaitForSeconds startup = new WaitForSeconds(1);
        WaitForFixedUpdate interval = new WaitForFixedUpdate();
        yield return startup;

        //startraum wird erstellt
        PlaceStartRoom();
        yield return interval;

        int iterations = Random.Range((int)iterationRange.x, (int)iterationRange.y);

        for (int i = 0; i < iterations; i++)
        {
            //plaziert ein raum von der liste
            Debug.Log("placed random room from list");
            PlaceRoom(); 
            yield return interval;
        }
        Debug.Log("Placed end room");
        yield return interval;

        Debug.Log("level generation completted");
        yield return new WaitForSeconds(3);
        ResetLevel();
    }

    //startraum wird gesetzt/erstellt
    void PlaceStartRoom()
    {
        Debug.Log("Placed StartRoom");
        startRoom = Instantiate(StartRoomPrefab) as Startroom;
        startRoom.transform.parent = this.transform;

        // Get Doorways and add them to List
        AddDoorwaysToList(startRoom, ref availableDoorways);

        startRoom.transform.position = Vector3.zero;
        startRoom.transform.rotation = Quaternion.identity;
    }
    void AddDoorwaysToList(Room room, ref List<Doorway> list)
    {
        foreach (Doorway doorway in room.doorways)
        {
            int r = Random.Range(0, list.Count);
            list.Insert(r, doorway);
        }
    }
    //räume/levelfragmente werden erstellt --> Geht noch nicht ganz
    void PlaceRoom()
    {
        Debug.Log("Placed Room");
        Room currentRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Count)]) as Room;
        currentRoom.transform.parent = this.transform;

        List<Doorway> allAvailableDoorways = new List<Doorway>(availableDoorways);
        List<Doorway> currentRoomDoorways = new List<Doorway>();
        AddDoorwaysToList(currentRoom, ref currentRoomDoorways);
        //bekommt alles doorways und fügt diese random zur liste der verfügbaren doorways hinzu
        AddDoorwaysToList(currentRoom, ref availableDoorways);
        bool roomPlaced = false;
        //geht alle verfügbaren doorways durch
        foreach (Doorway availableDoorway in allAvailableDoorways)
        {
            //geht alle verfügbaren doorways im jetztigen raum durch
            foreach (Doorway currentDoorway in currentRoomDoorways)
            {
                PositionRoomAtDoorway(ref currentRoom, currentDoorway, availableDoorway);
                //prüft auf overlap
                if (CheckRoomOverlap(currentRoom))
                {
                    continue;
                }
                roomPlaced = true;
                //fügt raum zur liste hinzu
                placedRooms.Add(currentRoom);
                //entfernt entsprechende doorways
                currentDoorway.gameObject.SetActive(false);
                availableDoorways.Remove(currentDoorway);

                availableDoorway.gameObject.SetActive(false);
                availableDoorways.Remove(availableDoorway);
                //bricht die schleife ab wenn der raum geplaced wurde
                break;
            }
            //bricht die schleife ab wenn der raum geplaced wurde
            if (roomPlaced)
            {
                break;
            }
        }
        //raum konnte nicht platziert werden, levelabschnitt wird zurück gesetzt und es wird erneut versucht.
        if (!roomPlaced)
        {
            Destroy(currentRoom.gameObject);
            ResetLevel();
        }
    }

    //Berechenung für Rotation des Raums
    void PositionRoomAtDoorway(ref Room room, Doorway roomDoorway, Doorway targetDoorway)
    {
        //reset für position und rotation
        room.transform.position = Vector3.zero;
        room.transform.rotation = Quaternion.identity;
        //rotiert den raum damit, er passend zum doorway ist
        Vector3 targetDoorwayEuler = targetDoorway.transform.eulerAngles;
        Vector3 roomDoorwayEuler = room.transform.eulerAngles;
        float deltaAngle = Mathf.DeltaAngle(roomDoorwayEuler.y, targetDoorwayEuler.y);
        Quaternion currentRoomTargetRotation = Quaternion.AngleAxis(deltaAngle, Vector3.up);
        room.transform.rotation = currentRoomTargetRotation * Quaternion.Euler(0, 180f, 0);
        //positionierung für den raum
        Vector3 roomPositionOffset = roomDoorway.transform.position - room.transform.position;
        room.transform.position = targetDoorway.transform.position - roomPositionOffset;
    }

    //Es wird geschaut ob sich die Räume overlapen
    bool CheckRoomOverlap(Room room)
    {
        Bounds bounds = room.RoomBounds;
        bounds.Expand(-0.1f);

        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.size / 2, room.transform.rotation, roomLayerMask);
        if (colliders.Length > 0)
        {
            //ignoriert collider mit jetzigen raum
            foreach (Collider c in colliders)
            {
                if (c.transform.parent.gameObject.Equals(room.gameObject))
                {
                    continue;
                }
                else
                {
                    Debug.LogError("Overlap detected");
                    return true;
                }
            }
        }
        return false;
    }

    //End raum wird geplaced --> ToDo
    void PlaceEndRoom()
    {
        Debug.Log("Placed EndRoom");

    }

    //Level wird zurücksetzt
    void ResetLevel()
    {
        Debug.LogError("Reset level generator");
        StopCoroutine("GenerateLevel");
        //startraum wird zerstört
        if (startRoom)
        {
            Destroy(startRoom.gameObject);
        }
        //endraum wird zerstört
        if (endRoom)
        {
            Destroy(endRoom.gameObject);
        }
        //räume werden zerstört
        foreach (Room room in placedRooms)
        {
            Destroy(room.gameObject);
        }
        //aufräumen der listen
        placedRooms.Clear();
        availableDoorways.Clear();
        //Startet generate Level
        StartCoroutine("GenerateLevel");
    }
}
