using UnityEditor;

namespace ProceduralMeshSupport
{
    [CustomEditor(typeof(PlaneMeshCreator))]
    public class PlaneMeshCreatorInspector : MeshCreatorInspector
    {
        protected override void OnInspectorGUIInternal()
        {
            base.OnInspectorGUIOriginal();
        }
    }
}