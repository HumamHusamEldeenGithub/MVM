using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchForUserPanel : MonoBehaviour
{
    [SerializeField]
    private Transform usersScrollView;

    [SerializeField]
    private TMP_InputField searchField;

    [SerializeField]
    private GameObject userRowPrefab;

    [SerializeField]
    private GameObject searchButton;

    [SerializeField]
    private Animator homeMenuPanel;

    [SerializeField]
    private Animator publicProfilePanel;

    private void Awake()
    {
        searchButton.transform.GetComponent<Button>().onClick.AddListener(SearchForUser);
    }

    private async void SearchForUser()
    {
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
        int childCount = usersScrollView.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = usersScrollView.GetChild(i);
            Destroy(child.gameObject);
        }

        string searchQuery = searchField.text.Trim();

        var users = await Server.SearchForUsers(new Mvm.SearchForUsersRequest
        {
            SearchInput = searchQuery
        });

        if (users != null)
        {
            foreach (var user in users.Users)
            {
                var element = Instantiate(userRowPrefab);
                var btn = element.transform.GetChild(0);
                btn.GetComponent<TMP_Text>().text = user.Username;
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    EventsPool.Instance.InvokeEvent(typeof(ShowProfileEvent), user.Id, GetComponent<Animator>());
                    GetComponent<Animator>().SetTrigger("FadeOut");
                    publicProfilePanel.SetTrigger("FadeIn");
                });
                element.transform.SetParent(usersScrollView);
            }
        }
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
    }
}
