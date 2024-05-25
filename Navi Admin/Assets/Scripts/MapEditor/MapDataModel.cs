using System;
using UnityEngine;
using Newtonsoft.Json;

namespace MapDataModel
{
    // -------------------------------------------
    // ------------- Map Data Model --------------
    // -------------------------------------------
    [Serializable]
    public class MapData
    {
        public string mapName;
        public string buildingName;
        public FloorData[] floors;
        public RoomTypeData[] roomTypes;
    }
    [Serializable]
    public class FloorData
    {
        public int floorLevel;
        public string floorName;
        public NodeData[] nodes;
        public WallData[] walls;
        public RoomData[] rooms;
        public ShapeData[] shapes;
        public QRCodeData[] qrCodes;
        public EventData[] events;
    }
    #region --- Map Components ---
    [Serializable]
    public class NodeData
    {
        public int nodeID;
        public SerializableVector3 nodePosition;
        public int[] neighborsNodes;
        public int[] walls;
        public int[] rooms;
    }
    [Serializable]
    public class WallData
    {
        public int wallID;
        public float wallLenght;
        public float wallWidth;
        public int startNode;
        public int endNode;
        public int[] rooms;
        public SerializableVector3 wallPosition;
        public EntranceData[] entrances;
        public MeshData renderData;
    }
    [Serializable]
    public class EntranceData
    {
        public int entranceID;
        public float entranceLenght;
        public SerializableVector3 entrancePosition;
        public SerializableVector3 startNodePosition;
        public SerializableVector3 endNodePosition;
    }
    [Serializable]
    public class RoomData
    {
        public int roomID;
        public string roomName;
        public int roomType;
        public int[] nodes;
        public int[] walls;
        public PolygonData polygonData;
        public MeshData renderData;
    }
    [Serializable]
    public class RoomTypeData
    {
        public int typeID;
        public string typeName;
        public bool searchNearestMode;
        public TranslatedText typeNameTranslation;
    }

    [Serializable]
    public class ShapeData
    {
        public int shapeID;
        public string shapeName;
        public SerializableVector3[] shapePoints;
        public SerializableVector3 shapePosition;
        public PolygonData polygonData;
        public MeshData renderData;
    }
    #endregion

    #region --- Polygon & Mesh ---
    [Serializable]
    public class PolygonData
    {
        public SerializableColor materialColor;
        public SerializableVector3[] vertices;
        public int[] triangles;
    }

    [Serializable]
    public class MeshData
    {
        public SerializableVector3[] vertices;
        public int[] triangles;
    }
    #endregion

    #region --- AR Features ---
    [Serializable]
    public class QRCodeData
    {
        public int qrCodeID;
        public string qrCodeName;
        public SerializableVector3 qrCodePosition;
        public SerializableQuaternionEuler qrCodeRotation;
    }

    [Serializable]
    public class EventData
    {
        public string eventName;
        public TranslatedText eventDescription;
        public TranslatedText[] eventDialogues;
        public SerializableVector3 eventPosition;
        public SerializableQuaternionEuler eventRotation;
    }
    #endregion

    #region --- Serializable Types ---
    [Serializable]
    public class TranslatedText
    {
        public string key;
        public string EnglishTranslation;
        public string SpanishTranslation;
    }

    [Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        [JsonIgnore]
        public Vector3 GetVector3
        {
            get
            {
                return new Vector3(x, y, z);
            }
        }

        public SerializableVector3(Vector3 v)
        {   // Constructor for a Vector3
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static SerializableVector3[] GetSerializableArray(Vector3[] vArray)
        {   // Convert a Vector3 array to a SerializableVector3 array
            SerializableVector3[] sArray = new SerializableVector3[vArray.Length];
            for (int i = 0; i < vArray.Length; i++)
                sArray[i] = new SerializableVector3(vArray[i]);
            return sArray;
        }

        public static Vector3[] GetVector3Array(SerializableVector3[] sArray)
        {   // Convert a SerializableVector3 array to a Vector3 array
            Vector3[] vArray = new Vector3[sArray.Length];
            for (int i = 0; i < sArray.Length; i++)
                vArray[i] = sArray[i].GetVector3;
            return vArray;
        }
    }

    [Serializable]
    public class SerializableQuaternionEuler
    {
        public float x;
        public float y;
        public float z;

        [JsonIgnore]
        public Quaternion GetQuaternion
        {
            get
            {
                return Quaternion.Euler(x, y, z);
            }
        }

        public SerializableQuaternionEuler(Quaternion q)
        {   // Constructor for a Quaternion
            Vector3 euler = q.eulerAngles;
            x = euler.x;
            y = euler.y;
            z = euler.z;
        }
    }

    [Serializable]
    public class SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        [JsonIgnore]
        public Color GetColor
        {
            get
            {
                return new Color(r, g, b, a);
            }
        }

        public SerializableColor(Color c)
        {   // Constructor for a Color
            r = c.r;
            g = c.g;
            b = c.b;
            a = c.a;
        }
    }
    #endregion
}
