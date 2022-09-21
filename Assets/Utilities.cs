using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Utilities
{
    public static string[] copyBanList = new string[] {
        "usedByComposite", "density", "tag"
    }; 

    public static Component copyComponent(Component comp, GameObject target)
    {
        System.Type type = comp.GetType();
        Component targetComp = target.GetComponent(type);
        if (targetComp == null)
            targetComp = target.AddComponent(type);

        // Copy over fields
        FieldInfo[] fields = type.GetFields();
        foreach (FieldInfo f in fields)
            f.SetValue(targetComp, f.GetValue(comp));

        // Copy over properties too (this feels really janky...)
        PropertyInfo[] props = type.GetProperties();
        foreach (PropertyInfo p in props)
        {
            // Banned properties
            bool banned = false;
            foreach (string s in copyBanList)
                if (s.Equals(p.Name))
                {
                    banned = true;
                    break;
                }
            if (banned)
                continue;

            if (p.SetMethod != null && p.GetMethod != null)
                p.SetValue(targetComp, p.GetValue(comp));
        }

        return targetComp;
    }
}
