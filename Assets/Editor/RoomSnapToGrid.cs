using Common;
using Object;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class RoomSnapToGrid
    {
        static RoomSnapToGrid()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var roomWidth = RoomDimensions.Width;
            var roomHeight = RoomDimensions.Height;

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Room>())
            {
                var room = Selection.activeGameObject;
                var pos = room.transform.position;
                var snappedX = Mathf.Round(pos.x / roomWidth) * roomWidth;
                var snappedY = Mathf.Round(pos.y / roomHeight) * roomHeight;
                var snappedPos = new Vector3(snappedX, snappedY, pos.z);

                if (snappedPos != pos)
                {
                    Undo.RecordObject(room.transform, "Snap Room To Grid");
                    room.transform.position = snappedPos;
                }
            }
        }
    }
}