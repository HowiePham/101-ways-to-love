using System;
using System.Reflection;
using Mimi.Prototypes.SaveLoad;

namespace Mimi.Prototypes.SaveLoad
{
    public class SaveRoot : ISaveRoot
    {
        public int SaveRootVersion => 0;

        public PlayerSave PlayerSave;

        public SaveRoot()
        {
            this.PlayerSave = new PlayerSave();
        }

        public void FixNullSaveContainers()
        {
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var fieldInfo in fields)
            {
                object o = fieldInfo.GetValue(this);

                if (o == null)
                {
                    fieldInfo.SetValue(this, Activator.CreateInstance(fieldInfo.FieldType));
                }
            }
        }
    }
}