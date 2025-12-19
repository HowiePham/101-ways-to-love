using Mimi.Reflection.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class InvertConditionGenerator : BaseGenerateObject<Invert>, IVisualConditionGenerator
    {
        private IVisualConditionGenerator wrappedGenerator;
        public override string PrefabAddress => "/Prefabs/VisualActions/Conditions/Invert.prefab";

        public InvertConditionGenerator(IVisualConditionGenerator wrappedGenerator)
        {
            this.wrappedGenerator = wrappedGenerator;
        }
        public VisualCondition Generate()
        {
            var wrapCondition = this.wrappedGenerator.Generate();
            var instance = GeneratePrefab();
            wrapCondition.transform.SetParent(instance.transform);
            instance.SetField("condition", wrapCondition, AccessModifier.Private);
            return instance;
        }
    }
}