using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Futboloid.Gameplay
{
    internal static class TweenAsync
    {
        public static async UniTask Await(Tween tween, CancellationToken ct)
        {
            if (tween == null || !tween.IsActive())
                return;

            using (ct.Register(() =>
                   {
                       if (tween.IsActive())
                           tween.Kill();
                   }))
            {
                await UniTask.WaitUntil(() => !tween.IsActive(), cancellationToken: ct);
            }
        }
    }
}
