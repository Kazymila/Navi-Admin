using System;
using UnityEngine;
using Newtonsoft.Json;

namespace MapDataModel
{
    #region --- Map Data Structures ---
    // -------------------------------------------
    // ----------------- Map Data ----------------
    // -------------------------------------------
    [Serializable]
    public class MapData
    {
        public string mapName;
        public string buildingName;
        public FloorData[] floors;
    }
    [Serializable]
    public class FloorData
    {
        public string floorName;
        public NodeData[] nodes;
        public WallData[] walls;
        public RoomData[] rooms;
        public ShapeData[] shapes;
        public QRCodeData[] qrCodes;
    }

    [Serializable]
    public class NodeData
    {
        public int nodeID;
        public SerializableVector3 nodePosition;
        public int[] neighborsNodes;
        public int[] polygons;
        public int[] walls;
    }
    [Serializable]
    public class WallData
    {
        public int wallID;
        public float wallLenght;
        public int startNode;
        public int endNode;
        public int[] polygons;
        public EntranceData[] entrances;
    }
    [Serializable]
    public class EntranceData
    {
        public string entranceName;
        public float entranceWidth;
        public SerializableVector3 entrancePosition;
        public SerializableVector3 startNodePosition;
        public SerializableVector3 endNodePosition;
    }
    [Serializable]
    public class RoomData
    {
        public int roomID;
        public string roomName;
        public int[] nodes;
        public int[] walls;
        public SerializableColor polygonColor;
        public SerializableVector3[] vertices;
        public int[] triangles;
    }
    [Serializable]
    public class ShapeData
    {
        public int shapeID;
        public string shapeName;
        public SerializableVector3[] vertices;
        public int[] triangles;
    }
    [Serializable]
    public class QRCodeData
    {
        public string qrCodeName;
        public SerializableVector3 qrCodePosition;
        public SerializableQuaternionEuler qrCodeRotation;
        public string qrCodeText;
    }
    #endregion

    #region --- 3D Map Data Structures ---
    // ----------------------------------------------
    // ----------------- AR Map Data ----------------
    // ----------------------------------------------

    [Serializable]
    public class ARMapData
    {
        public string mapName;
        public string buildingName;
        public ARFloorData[] floors;
    }
    [Serializable]
    public class ARFloorData
    {
        public string floorName;
        public WallRenderData[] walls;
        public RoomRenderData[] rooms;
        public ShapeRenderData[] shapes;
    }

    [Serializable]
    public class WallRenderData
    {
        public int wallID;
        public SerializableVector3[] vertices;
        public int[] triangles;
    }
    public class RoomRenderData
    {
        public int roomID;
        public string roomName;
        public SerializableVector3[] entrancesPoints;
        public SerializableColor polygonColor;
        public SerializableVector3[] vertices;
        public int[] triangles;
    }

    [Serializable]
    public class ShapeRenderData
    {
        public int shapeID;
        public string shapeName;
        public SerializableVector3[] vertices;
        public int[] triangles;
    }
    #endregion

    #region --- Serializable Unity Types ---
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
