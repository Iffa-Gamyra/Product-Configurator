using System;
using UnityEngine.UIElements;

public class WelcomeScreenManager
{
    private readonly Button startButton;
    private Action onStart;

    public WelcomeScreenManager(VisualElement root)
    {
        startButton = root.Q<Button>("start");
    }

    public void BindStart(Action action)
    {
        if (startButton == null) return;

        if (onStart != null)
            startButton.clicked -= onStart;

        onStart = action;
        startButton.clicked += onStart;
    }
}