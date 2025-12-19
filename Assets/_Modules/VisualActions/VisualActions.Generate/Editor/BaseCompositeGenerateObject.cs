using System.Collections.Generic;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor
{
    public abstract class BaseCompositeGenerateObject<T,U> : BaseGenerateObject<T> where T : Object
    {
        protected List<U> generators = new List<U>();
        
        public BaseCompositeGenerateObject<T,U> AddGenerator(U generator)
        {
            generators.Add(generator);
            return this;
        }

        public void SetGenerators(List<U> generators)
        {
            this.generators = generators;
        }
    }
}