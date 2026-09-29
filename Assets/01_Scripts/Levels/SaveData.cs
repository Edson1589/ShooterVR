using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version;
    public string nextScene;
    public bool gameCompleted;
    public List<LevelResult> results = new List<LevelResult>();

    [Serializable]
    public class LevelResult
    {
        public string scene;
        public int score;
        public int defeatedEnemies;
        public int elapsedSeconds;
    }
}
