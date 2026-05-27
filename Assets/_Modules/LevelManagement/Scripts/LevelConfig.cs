using System.Collections.Generic;

namespace Mimi.Prototypes
{
    public class LevelConfig
    {
        private readonly HashSet<string> levels;
        public int MaxLevel { private set; get; }

        public LevelConfig()
        {
            this.levels = new HashSet<string>();
        }

        public void ParseConfig(string config)
        {
            this.levels.Clear();
            var splits = config.Split(',');

            int max = 0;
            foreach (var levelId in splits)
            {
                int levelOrder = int.Parse(levelId);
                if (levelOrder > max)
                {
                    max = levelOrder;
                }

                this.levels.Add(levelId);
            }

            MaxLevel = max;
        }

        public List<string> GetLevels()
        {
            List<string> levelStrings = new List<string>();

            foreach (string levelString in this.levels)
            {
                levelStrings.Add(levelString);
            }

            return levelStrings;
        }

        public bool HasLevel(string levelId)
        {
            return this.levels.Contains(levelId);
        }
    }
}