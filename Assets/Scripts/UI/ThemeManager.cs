using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class ThemeManager : MonoBehaviour
{
    public PanelSettings Landscape;
    public PanelSettings Portrait;

    private void Start()
    {
        if (TryGetComponent(out UIDocument document))
        {
            var root = document.rootVisualElement;

            root.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (Screen.width > Screen.height)
                {
                    if (Landscape)
                        document.panelSettings = Landscape;
                }
                else
                {
                    if (Portrait)
                        document.panelSettings = Portrait;
                }
            });
        }
    }
}