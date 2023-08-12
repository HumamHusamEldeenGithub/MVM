using System.Collections;
using System.Collections.Generic;
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
    private PublicProfilePanel publicProfilePanel;

    private void Awake()
    {
        searchButton.transform.GetComponent<Button>().onClick.AddListener(SearchForUser);
    }

    private async void SearchForUser()
    {
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

        foreach (var user in users.Users)
        {
            var element = Instantiate(userRowPrefab);
            var btn = element.transform.GetChild(0);
            btn.GetComponent<TMP_Text>().text = user.Username;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                publicProfilePanel.ShowProfile(user.Id, this.transform);
            });
            element.transform.SetParent(usersScrollView);
        }
    }
}
