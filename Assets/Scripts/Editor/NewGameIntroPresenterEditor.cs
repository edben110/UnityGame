using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NewGameIntroPresenter))]
public class NewGameIntroPresenterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NewGameIntroPresenter presenter = (NewGameIntroPresenter)target;
        if (presenter == null)
        {
            return;
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox(
            "Usa «Editar en escena» para ver la UI en Scene View. " +
            "Selecciona Background, Title o DialoguePanel y asigna sprites / RectTransform.",
            MessageType.Info);

        EditorGUILayout.LabelField("Editar en Scene View", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Tutorial"))
            {
                presenter.EditorBeginEditTutorial();
            }

            if (GUILayout.Button("Prólogo"))
            {
                presenter.EditorBeginEditPrologue();
            }

            if (GUILayout.Button("Ocultar"))
            {
                presenter.EditorHidePreview();
            }
        }

        if (GUILayout.Button("Reconfigurar UI bajo Canvas"))
        {
            SetupMenuNewGameIntro.Setup();
        }
    }
}
