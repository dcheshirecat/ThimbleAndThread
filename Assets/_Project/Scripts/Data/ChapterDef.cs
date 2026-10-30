using UnityEngine;

// One chapter of the story. Each opens a new craft (see SkillDef.chapter) and sets goals; meeting
// them moves the story on to the next chapter. Create via Assets > Create > Craft > Chapter.
// Lives in its own file: Unity only links a ScriptableObject asset to its class when the file name matches.
[CreateAssetMenu(menuName = "Craft/Chapter")]
public class ChapterDef : ScriptableObject
{
    public int number;                     // 1, 2, 3...
    public string title;
    [TextArea(3, 10)] public string intro;   // told when the chapter begins
    public string inkScene;                // optional: the Ink knot played when it begins (overrides intro)

    [Header("Goals to reach the next chapter (0 = no goal)")]
    public int deliveries;                 // orders handed over, in total
    public int patternsSold;               // patterns sold off the display, in total
    public int friendLevel;                // someone's friendship reaches this level
    [TextArea(2, 4)] public string goalText;   // how the Journal describes the goals
}
