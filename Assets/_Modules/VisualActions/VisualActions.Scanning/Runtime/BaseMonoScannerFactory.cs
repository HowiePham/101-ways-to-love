using UnityEngine;

namespace Mimi.VisualActions.Scanning
{
    public abstract class BaseMonoScannerFactory : MonoBehaviour, IScannerFactory
    {
        public abstract IScanner CreateScanner();
    }
}