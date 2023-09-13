using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SwitchButton : MonoBehaviour
{
    [SerializeField]
    private Button option_1;

    [SerializeField]
    private Button option_2;

    [SerializeField]
    private Color selectedColor;

    [SerializeField]
    private Color unselectedColor;

    private TextMeshProUGUI option1Text;
    private TextMeshProUGUI option2Text;

    public UnityEvent<int> onValueChange;

    public int value
    {
        get; set;
    }

    private void Awake()
    {
        option1Text = option_1.GetComponent<TextMeshProUGUI>();
        option2Text = option_2.GetComponent<TextMeshProUGUI>();

        option_1?.onClick.AddListener(OnOptionOne);
        option_2?.onClick.AddListener(OnOptionTwo);
    }

    private void OnOptionOne()
    {
        value = 0;
        option1Text.color = selectedColor;
        option2Text.color = unselectedColor;

        onValueChange?.Invoke(value);
    }

    private void OnOptionTwo()
    {
        value = 1;
        option1Text.color = unselectedColor;
        option2Text.color = selectedColor;

        onValueChange?.Invoke(value);
    }

    public void SetSelectedOption(int option)
    {
        if(option == 1)
        {
            OnOptionOne();
        }
        else
        {
            OnOptionTwo();
        }
    }
}
