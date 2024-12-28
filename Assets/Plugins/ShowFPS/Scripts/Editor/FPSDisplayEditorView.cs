using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Utility;

[CustomEditor(typeof(FPSDisplay))]
public class FPSDisplayEditorView : Editor
{
    public VisualTreeAsset TreeAsset;

    private FPSDisplay _fPSDisplay;

    private void OnEnable()
    {
        _fPSDisplay = (FPSDisplay)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        if (!TreeAsset)
            return base.CreateInspectorGUI();

        VisualElement root = new VisualElement();
        TreeAsset.CloneTree(root);

        Toggle fpsToggle = root.Q<Toggle>("toggle_showfps");
        
        fpsToggle.RegisterCallback<ChangeEvent<bool>>(evt => _fPSDisplay.SetActiveUI(evt.newValue));
        
        return root;
    }
}