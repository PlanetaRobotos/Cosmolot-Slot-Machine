using UnityEngine;

namespace Mechanics
{
    public class PrivacyPolicyUI : MonoBehaviour
    {
        [SerializeField] private string policy;

        public void Policy() => Application.OpenURL(policy);
    }
}