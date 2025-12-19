using System.Linq;

namespace Mimi.VisualActions.Attribute.Editor
{
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[DrawerPriority(0, 100)]
public class OnAddRemoveItemDrawer<TList, TElement> : OdinValueDrawer<TList> 
    where TList : IList<TElement>
{
    private MethodInfo onAddMethod;
    private MethodInfo onRemoveMethod;
    private int lastCount = -1;
    private List<TElement> previousSnapshot;

    protected override void Initialize()
    {
        base.Initialize();

        var member = this.Property.Info.GetMemberInfo();
        var parentType = this.Property.ParentType;

        var onAddAttr = member.GetCustomAttribute<OnAddItemAttribute>();
        var onRemoveAttr = member.GetCustomAttribute<OnRemoveItemAttribute>();

        if (onAddAttr != null)
        {
            onAddMethod = FindMethod(parentType, onAddAttr.MethodName, typeof(void), new[] { typeof(TElement) });
        }
        else
        {
            return;
        }


        if (onRemoveAttr != null)
        {
            onRemoveMethod = FindMethod(parentType, onRemoveAttr.MethodName, typeof(void), new[] { typeof(TElement) });

        }
        else
        {
            return;
        }

        // Gọi CheckChange khi list thay đổi
        this.Property.ValueEntry.OnValueChanged += CheckChange;
        var list = this.ValueEntry.SmartValue;
        previousSnapshot = new List<TElement>(list);
        lastCount = list.Count;
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        this.CallNextDrawer(label);
    }

    private void CheckChange(int index)
    {
        var list = this.ValueEntry.SmartValue;
        if (list == null) return;

        int count = list.Count;
        var parent = this.Property.ParentValues[0];
        // Lần đầu: lưu snapshot
        if (previousSnapshot == null)
        {
            previousSnapshot = new List<TElement>(list);
            lastCount = count;
            return;
        }

        // === THÊM PHẦN TỬ ===
        if (count > lastCount)
        {
            // Odin thêm vào CUỐI → lấy phần tử cuối
            var newItem = list[count - 1];
            onAddMethod?.Invoke(parent, new object[] { newItem });
        }
        // === XÓA PHẦN TỬ ===
        else if (count < lastCount)
        {
            // Tìm phần tử bị xóa bằng cách so sánh 2 list
            var currentSet = new HashSet<TElement>(list);
            var removedItem = previousSnapshot.FirstOrDefault(x => !currentSet.Contains(x));
            
            if (removedItem != null)
                onRemoveMethod?.Invoke(parent, new object[] { removedItem });
        }

        // Cập nhật snapshot
        lastCount = count;
        previousSnapshot = new List<TElement>(list);
    }

    private static MethodInfo FindMethod(System.Type type, string methodName, System.Type returnType, System.Type[] parameterTypes)
    {
        var method = type.GetMethod(methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, parameterTypes, null);

        if (method == null || (returnType != null && method.ReturnType != returnType))
        {
            Debug.LogWarning($"[OnAddRemoveItem] Method '{methodName}' not found in {type.Name}");
            return null;
        }
        return method;
    }
}
#endif
}