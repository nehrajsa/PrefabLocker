using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nehrajsa.PrefabLocker.Editor
{
    [CustomEditor(typeof (PrefabLocker))]
    public class PrefabLockerEditor : UnityEditor.Editor
    {
        private static readonly HashSet<string> AllowedOverrides = new HashSet<string>()
        {
            "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z",
            "m_LocalRotation.w", "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z",
            "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z",
            "m_LocalEulerAnglesHint.x", "m_LocalEulerAnglesHint.y", "m_LocalEulerAnglesHint.z",
            "m_ConstrainProportionsScale",
            "m_Name",
            "m_Enabled",
            "m_IsActive", 
        };

        private SerializedProperty _autoRemoveAddedComponentOverrides;
        private SerializedProperty _autoRemoveAddedGameObjectOverrides;
        private int _instanceId;

        private void OnEnable()
        {
            _autoRemoveAddedComponentOverrides = serializedObject.FindProperty("_autoRemoveAddedComponentOverrides");
            _autoRemoveAddedGameObjectOverrides = serializedObject.FindProperty("_autoRemoveAddedGameObjectOverrides");
            
            _instanceId = target is PrefabLocker prefabLocker ? prefabLocker.gameObject.GetInstanceID() : 0;
        }

        private void OnDisable()
        {
            if (target == null)
            {
                var gameObject = EditorUtility.InstanceIDToObject(_instanceId) as GameObject;
                Unlock(gameObject);
                _instanceId = 0;
            }
        }
        
        public override void OnInspectorGUI()
        {
            var prefabLockerInstance = target as PrefabLocker;
            if (!prefabLockerInstance) return;
            
            if (PrefabStageUtility.GetCurrentPrefabStage())
            {
                serializedObject.Update();
                _autoRemoveAddedComponentOverrides.boolValue = EditorGUILayout.ToggleLeft("Auto Remove Added Component Overrides", _autoRemoveAddedComponentOverrides.boolValue);
                _autoRemoveAddedGameObjectOverrides.boolValue = EditorGUILayout.ToggleLeft("Auto Remove Added GameObject Overrides", _autoRemoveAddedGameObjectOverrides.boolValue);
                serializedObject.ApplyModifiedProperties();
                
                Unlock(prefabLockerInstance.gameObject);
                return;
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("In Play Mode ...");
                Unlock(prefabLockerInstance.gameObject);
                return;
            }

            EditorGUILayout.LabelField("GameObject is locked.");
            
            if (ProcessOverrides(prefabLockerInstance.gameObject, prefabLockerInstance.AutoRemoveAddedComponentOverrides, prefabLockerInstance.AutoRemoveAddedGameObjectOverrides))
            {
                var warningStyle = new GUIStyle(GUI.skin.label);
                warningStyle.normal.textColor = warningStyle.hover.textColor = warningStyle.active.textColor = Color.red;
    
                EditorGUILayout.LabelField("Prefab has overrides!", warningStyle);
            }

            Lock(prefabLockerInstance.gameObject);
        }

        private bool ProcessOverrides(GameObject g, bool autoRemoveAddedComponentOverrides, bool autoRemoveAddedGameObjectOverrides)
        {
            if (!PrefabUtility.HasPrefabInstanceAnyOverrides(g, false)) return false;
            
            var hasOverrides = false;
            foreach (var propertyModification in PrefabUtility.GetPropertyModifications(g))
            {
                if (AllowedOverrides.Contains(propertyModification.propertyPath)) continue;

                hasOverrides = true;
                break;
            }
                
            foreach (var addedComponent in PrefabUtility.GetAddedComponents(g))
            {
                if (autoRemoveAddedComponentOverrides)
                {
                    DestroyImmediate(addedComponent.instanceComponent);
                    continue;
                }
                
                hasOverrides = true;
                break;
            }
                
            foreach (var addedGameObject in PrefabUtility.GetAddedGameObjects(g))
            {
                if (autoRemoveAddedGameObjectOverrides)
                {
                    DestroyImmediate(addedGameObject.instanceGameObject);
                    continue;
                }
                
                hasOverrides = true;
                break;
            }

            return hasOverrides;
        }

        private void Lock(GameObject g)
        {
            foreach (var component in g.GetComponents<Component>())
            {
                if (component is Transform)
                {
                    component.hideFlags = HideFlags.None;
                    continue;
                }
                
                if (component is PrefabLocker)
                {
                    component.hideFlags = HideFlags.None | HideFlags.DontSaveInBuild;
                    continue;
                }

                if (PrefabUtility.IsAddedComponentOverride(component))
                {
                    component.hideFlags = HideFlags.None;
                    continue;
                }
                
                component.hideFlags = HideFlags.HideInInspector;
            }
            
            foreach (Transform child in g.transform)
            {
                if (PrefabUtility.IsAddedGameObjectOverride(child.gameObject))
                {
                    child.hideFlags = HideFlags.None;
                    continue;
                }
                
                child.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
        }

        private void Unlock(GameObject g)
        {
            if (!g) return;
            
            foreach (var component in g.GetComponents<Component>())
            {
                component.hideFlags = HideFlags.None;
                
                if (component is PrefabLocker)
                {
                    component.hideFlags |= HideFlags.DontSaveInBuild;
                }
            }
            
            foreach (Transform child in g.transform)
            {
                child.hideFlags = HideFlags.None;
            }
        }
    }
}