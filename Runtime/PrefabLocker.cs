using UnityEngine;

namespace Nehrajsa.PrefabLocker
{
    public class PrefabLocker : MonoBehaviour
    {
        [SerializeField] private bool _autoRemoveAddedComponentOverrides;
        [SerializeField] private bool _autoRemoveAddedGameObjectOverrides;

        public bool AutoRemoveAddedComponentOverrides => _autoRemoveAddedComponentOverrides;
        public bool AutoRemoveAddedGameObjectOverrides => _autoRemoveAddedGameObjectOverrides;
        
        private void Start()
        {
            Debug.LogError("This should not be available in the build!");
        }
    }
}
