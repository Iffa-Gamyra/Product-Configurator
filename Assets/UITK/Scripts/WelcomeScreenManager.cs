using System;
using UnityEngine.UIElements;

public class WelcomeScreenManager
{
    public Action Start { set => start_but.clicked += value; }

    private Button start_but;

    public WelcomeScreenManager(VisualElement root)
    {
        start_but = root.Q<Button>("start");
    }
}