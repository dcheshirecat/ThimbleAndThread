using System;
using UnityEngine;

// The little animated scene each tutorial page shows until it has its own art.
// Append new values at the end: the tutorial asset stores these as numbers.
public enum TutorialAnim { Welcome, DayAndNight, Customers, Stitching, Gestures, OrderBook, Market }

[Serializable]
public class TutorialPage
{
    public string title;
    [TextArea(3, 8)] public string text;
    [Tooltip("Optional: your illustration for this page. Leave empty to show the animated placeholder.")]
    public Sprite art;
    public TutorialAnim animation;
}

// The "How to Play" tutorial, shown the first time the app is opened and from the title screen.
// Edit the pages' words in the Inspector; drop in art when it's ready. There is one: Data/Tutorial.asset.
// Lives in its own file: Unity only links a ScriptableObject asset to its class when the file name matches.
[CreateAssetMenu(menuName = "Craft/Tutorial")]
public class TutorialDef : ScriptableObject
{
    public TutorialPage[] pages = new TutorialPage[0];
}
