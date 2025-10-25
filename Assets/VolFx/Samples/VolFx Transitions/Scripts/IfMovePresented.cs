using UnityEngine;
using UnityEngine.Events;

namespace VolFx
{
    public class IfMovePresented : MonoBehaviour
    {
        public UnityEvent _onInvoke;

        private void OnEnable()
        {
            var hasMove = FindObjectOfType<Move>();

            if (hasMove)
                _onInvoke.Invoke();
        }
    }
}