using UnityEngine;

public static class Utility
{    
     /// <summary>
     /// Y축 180도 반전 회전
     /// </summary>
     /// <param name="transform">반전 시킬 객체</param>
    public static void ToggleYRotation(Transform transform)
    {
        Vector3 currentEuler = transform.localEulerAngles;

        float currentY = currentEuler.y % 360f;
        if (currentY < 0) currentY += 360f;

        float targetY = (currentY + 180f) % 360f;

        currentEuler.y = targetY;
        transform.localEulerAngles = currentEuler;
    }

}
