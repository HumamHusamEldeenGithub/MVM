using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup[] rows;

    private int participantCount;
    private List<Image> screens = new List<Image>();
    private List<RenderTexture> renderTextures = new List<RenderTexture>();
    private int n_rows, n_cols;

    private void CreateNewScreen(RoomSpaceController.RoomRenderTexture rt)
    {
        renderTextures.Add(rt.renderTexture);
        participantCount++;
        ArrangeParticipantViews(Screen.width, Screen.height);
    }
    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(CreateNewScreenEvent), new Action<RoomSpaceController.RoomRenderTexture>(CreateNewScreen));
    }

    private void ArrangeParticipantViews(int screenWidth, int screenHeight)
    {
        Initialize();
        TurnOffScreens();

        if (participantCount == 0)
            return;

        n_rows = n_cols = Mathf.CeilToInt(Mathf.Sqrt(participantCount));

        int width = screenWidth / n_cols;

        int height = screenHeight / n_rows;

        int cnt = 0;
        for (int i = 0; i < n_rows && cnt < participantCount; i++)
        {
            rows[i].gameObject.SetActive(true);
            rows[i].cellSize = new Vector2(width, height);
            for (int j = 0; j < n_cols && cnt < participantCount; j++)
            {
                rows[i].transform.GetChild(j).gameObject.SetActive(true);
                rows[i].transform.GetChild(j).GetComponent<Image>().material.mainTexture = renderTextures[cnt];
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
        foreach (var el in rows)
        {
            el.gameObject.SetActive(false);
        }

    }

    private void Initialize()
    {
        if (rows.Length > 0 && screens.Count == 0)
        {
            foreach (var element in rows)
            {
                for (int i = 0; i < element.transform.childCount; i++)
                {
                    screens.Add(element.transform.GetChild(i).GetComponent<Image>());
                }
            }
        }
    }
}