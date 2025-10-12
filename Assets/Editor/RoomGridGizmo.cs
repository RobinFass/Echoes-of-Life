using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public class RoomGridGizmo
    {
        static RoomGridGizmo()
        {
            // Uncomment the line below to enable the grid gizmo in the Scene view
            // SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var roomWidth = RoomDimensions.Width;
            var roomHeight = RoomDimensions.Height;
            
            var gridCount = 10;
            Handles.color = Color.green;
            for (var x = -gridCount; x <= gridCount; x++)
            {
                for (var y = -gridCount; y <= gridCount; y++)
                {
                    var pos = new Vector3(x * roomWidth, y * roomHeight, 0);
                    Handles.DrawWireCube(pos, new Vector3(roomWidth, roomHeight, 0));
                }
            }
        }
    }
}