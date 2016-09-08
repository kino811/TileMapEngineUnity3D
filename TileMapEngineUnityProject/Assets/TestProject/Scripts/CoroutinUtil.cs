using UnityEngine;
using System.Collections;
using System;

namespace Kino.Util {
    public static class CoroutineUtil {
        public static IEnumerator DelayRoutine(float delaySeconds, Action routine) {
            yield return new WaitForSeconds(delaySeconds);

            routine();

            yield return null;
        }
    }
}