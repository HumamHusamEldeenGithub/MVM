using Mono.Cecil;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomGridLayout : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup[] rows;

    [Range(0, 9)]
    [SerializeField] int participantCount;

    private List<Transform> screens;
    private int n_rows, n_cols;

    private void Start()
    {
        // Call this method whenever the participant count changes
        ArrangeParticipantViews(Screen.width, Screen.height);
    }

    private void ArrangeParticipantViews(int screenWidth, int screenHeight)
    {
        TurnOffScreens();

        if (participantCount == 0)
            return;

        n_rows = n_cols = Mathf.CeilToInt(Mathf.Sqrt(participantCount));

        int width = screenWidth / n_cols;

        int height = screenHeight / n_rows;

        int cnt = 0;
        for(int i = 0;i < n_rows && cnt < participantCount; i++)
        {
            rows[i].gameObject.SetActive(true);
            rows[i].cellSize = new Vector2(width, height);
            for(int j = 0;j < n_cols && cnt < participantCount;j++)
            {
                rows[i].transform.GetChild(j).gameObject.SetActive(true);
                cnt++;
            }
        }

    }

    private void TurnOffScreens()
    {
        foreach (var element in screens)
        {
            element.gameObject.SetActive(false);
        }
        foreach(var el in rows)
        {
            el.gameObject.SetActive(false);
        }

    }

    private void OnValidate()
    {
        if(rows.Length > 0)
        {
            screens = new List<Transform>();
            foreach(var element in rows)
            {
                for(int i = 0;i < element.transform.childCount; i++)
                {
                    screens.Add(element.transform.GetChild(i).transform);
                }
            }
        }
        ArrangeParticipantViews((int)Handles.GetMainGameViewSize().x, (int)Handles.GetMainGameViewSize().y);
    }
}
