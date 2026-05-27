using System.Collections.Generic;
using System.Linq;

namespace Mimi.Prototypes.LevelManagement
{
    public class SheetAngelSkinRepo
    {
        private readonly IReadOnlyList<SheetAngelSkinModel> skins;
        private readonly Dictionary<string, SheetAngelSkinModel> skinsById;

        public SheetAngelSkinRepo(IEnumerable<SheetAngelSkinModel> models)
        {
            this.skins = models.ToList();
            this.skinsById = new Dictionary<string, SheetAngelSkinModel>();

            foreach (SheetAngelSkinModel skinModel in this.skins)
            {
                if (this.skinsById.ContainsKey(skinModel.Id))
                {
                    continue;
                }

                this.skinsById.Add(skinModel.Id, skinModel);
            }
        }

        public string GetSkinType(string skinId)
        {
            if (this.skinsById.ContainsKey(skinId))
            {
                return this.skinsById[skinId].SkinType;
            }

            return "";
        }

        public bool IsContainSkin(string skinId)
        {
            return this.skinsById.ContainsKey(skinId);
        }

        public IReadOnlyList<SheetAngelSkinModel> GetAll() => this.skins;
    }
}