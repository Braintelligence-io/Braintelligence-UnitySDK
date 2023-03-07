using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Braintelligence.Editor
{
    public class GameSelectionState
    {
        private readonly VisualElement _root;
        private readonly DropdownField _dropDown;
        private readonly Label _label;

        public GameSelectionState(VisualElement root)
        {
            _root = root.Q<VisualElement>("game-selection");
            _dropDown = _root.Q<DropdownField>("games-dropdown");
            _dropDown.choices = new List<string> { "First Game", "Another Game", "Different Game" };
            _dropDown.RegisterValueChangedCallback(OnDropdownSelectionChanged);
            _label = _root.Q<Label>("gameinfo-label");
            _root.style.display = DisplayStyle.None;
        }

        private void OnDropdownSelectionChanged(ChangeEvent<string> evt)
        {
            _label.text = $"{evt.newValue}:\n{Guid.NewGuid().ToString()}";
        }

        public void Init()
        {
            _root.style.display = DisplayStyle.Flex;

        }
    }
}