using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Audio;

namespace Mimi.VisualActions.Generate.Editor
{
    public class PlayAudioGenerator : BaseGenerateObject<PlayAudio>, IVisualActionGenerator
    {
        private string soundKey;
        public override string PrefabAddress => "/Prefabs/VisualActions/PlayAudio.prefab";

        public PlayAudioGenerator(string soundKey)
        {
            this.soundKey = soundKey;
        }
        public VisualAction Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("soundKey", soundKey, AccessModifier.Private);
            return instance;
        }
    }
}