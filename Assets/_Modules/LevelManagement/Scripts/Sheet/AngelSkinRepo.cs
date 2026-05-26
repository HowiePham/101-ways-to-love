using System.Collections.Generic;
using System.Linq;

namespace Mimi.Prototypes.LevelManagement
{
    public class AngelSkinRepo
    {
        private readonly IReadOnlyList<SheetAngelSkinModel> skins;

        public AngelSkinRepo(IEnumerable<SheetAngelSkinModel> models)
        {
            this.skins = models.ToList();
        }

        public IReadOnlyList<SheetAngelSkinModel> GetAll() => this.skins;
    }
}
