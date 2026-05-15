using UnityEngine;

public class MapStateObject : MonoBehaviour
{
    [SerializeField] private string requiredFlag;
    [SerializeField] private bool invertResult;

    private void OnEnable()
    {
        Refresh();

        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (string.IsNullOrWhiteSpace(requiredFlag) || StoryState.Instance == null)
        {
            gameObject.SetActive(true);
            return;
        }

        bool hasFlag = StoryState.Instance.HasFlag(requiredFlag);
        bool shouldShow = invertResult ? !hasFlag : hasFlag;
        gameObject.SetActive(shouldShow);
    }
}
