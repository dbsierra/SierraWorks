using UnityEngine;
using UnityEditor;

namespace SierraWorks.Parameter.Editor
{
    public static class ParameterHubMenuItems
    {
        [MenuItem("GameObject/SierraWorks/PARAM/Parameter Sender", false, 10)]
        private static void CreateParameterSender(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("ParameterSender");
            go.AddComponent<ParameterSender>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/SierraWorks/PARAM/Parameter Receiver", false, 10)]
        private static void CreateParameterReceiver(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("ParameterReceiver");
            go.AddComponent<ParameterReceiver>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/SierraWorks/PARAM/Preset Writer", false, 10)]
        private static void CreatePresetWriter(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("PresetWriter");
            go.AddComponent<PresetWriter>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/SierraWorks/PARAM/Preset Loader", false, 10)]
        private static void CreatePresetLoader(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("PresetLoader");
            go.AddComponent<PresetLoader>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/SierraWorks/PARAM/Serializer", false, 10)]
        private static void CreateSerializer(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("ParameterSetSerializer");
            go.AddComponent<ParameterSetSerializer>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/SierraWorks/PARAM/Example Controller", false, 10)]
        private static void CreateExampleController(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("ParameterSetExample");
            go.AddComponent<ParameterSetExample>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}
